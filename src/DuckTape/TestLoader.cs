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
    public static void Load(string file)
    {
        var source = File.ReadAllText(file);
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var compilation = CSharpCompilation.Create(
            "ducktape_" + Path.GetFileNameWithoutExtension(file),
            new[] { tree },
            References(),
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        using var ms = new MemoryStream();
        var emit = compilation.Emit(ms);
        if (!emit.Success)
        {
            var errors = string.Join("\n", emit.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new InvalidOperationException($"ducktape: failed to compile {file}\n{errors}");
        }

        ms.Position = 0;
        var asm = AssemblyLoadContext.Default.LoadFromStream(ms);
        var entry = asm.EntryPoint!;
        var argv = entry.GetParameters().Length == 0
            ? Array.Empty<object?>()
            : new object?[] { Array.Empty<string>() };
        entry.Invoke(null, argv);
    }

    static List<MetadataReference> References()
    {
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "";
        var refs = tpa
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();
        refs.Add(MetadataReference.CreateFromFile(typeof(Test).Assembly.Location));
        return refs;
    }
}
