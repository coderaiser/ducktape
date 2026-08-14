using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DuckTape;

/// <summary>
/// Compiles a top-level-statements spec file on the fly with Roslyn and runs
/// its entry point, which registers its tests into the shared Test registry.
/// </summary>
public static class TestLoader
{
    const string Usings = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;

    public static void Load(string file)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        sw.Stop();
        var t0 = sw.Elapsed.TotalMilliseconds;
        sw.Restart();
        var source = Usings + File.ReadAllText(file);
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        sw.Stop();
        var tParse = sw.Elapsed.TotalMilliseconds;
        sw.Restart();
        var refs = References();
        sw.Stop();
        var tRefs = sw.Elapsed.TotalMilliseconds;
        sw.Restart();
        var compilation = CSharpCompilation.Create(
            "ducktape_" + Path.GetFileNameWithoutExtension(file),
            new[] { tree },
            refs,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        using var ms = new MemoryStream();
        var emit = compilation.Emit(ms);
        sw.Stop();
        var tEmit = sw.Elapsed.TotalMilliseconds;
        if (!emit.Success)
        {
            var errors = string.Join("\n", emit.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new InvalidOperationException($"ducktape: failed to compile {file}\n{errors}");
        }

        sw.Restart();
        ms.Position = 0;
        var asm = AssemblyLoadContext.Default.LoadFromStream(ms);
        var entry = asm.EntryPoint!;
        var argv = entry.GetParameters().Length == 0
            ? Array.Empty<object?>()
            : new object?[] { Array.Empty<string>() };
        entry.Invoke(null, argv);
        sw.Stop();
        System.Console.Error.WriteLine($"[timing] {Path.GetFileName(file)} parse={tParse:F0}ms refs={tRefs:F0}ms emit={tEmit:F0}ms load+invoke={sw.Elapsed.TotalMilliseconds:F0}ms");
    }

    static List<MetadataReference> References()
    {
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "";
        var refs = tpa
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();
        refs.Add(MetadataReference.CreateFromFile(typeof(Tests).Assembly.Location));
        return refs;
    }
}
