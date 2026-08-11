using DuckTape.Formatter;

namespace DuckTape.Cli;

public record CliDependencies(
    Action<string> Load,
    Func<List<TestDefinition>> Tests,
    Func<string, TextWriter, IFormatter> Resolve);

public static class CliRunner
{
    public static readonly CliDependencies Default = new(
        TestLoader.Load,
        () => Test.All,
        (format, stream) => FormatterResolver.Resolve(format, format == "progress-bar" ? Console.Error : stream));

    public static int Run(string[] argv) => Execute(argv, Console.Out, Console.Error, Default);

    public static int Run(string[] argv, TextWriter stdout, TextWriter stderr) =>
        Execute(argv, stdout, stderr, Default);

    public static int Execute(string[] argv, TextWriter stdout, TextWriter stderr, CliDependencies deps)
    {
        Args args;
        try { args = ArgParser.Parse(argv); }
        catch (InvalidFormatException ex)
        {
            stderr.WriteLine(ex.Message);
            return ExitCodes.InvalidOption;
        }

        if (args.Help) { stdout.Write(Help.Text); return ExitCodes.Ok; }
        if (args.Version) { stdout.WriteLine("0.1.0"); return ExitCodes.Ok; }

        if (args.NoCheckDuplicates)
            Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_DUPLICATES", "0");
        if (args.NoCheckAssertionsCount)
            Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", "0");

        var files = args.Patterns
            .SelectMany(Glob.Expand)
            .Distinct()
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        if (files.Count == 0)
        {
            stderr.WriteLine("ducktape: no test files matched");
            return ExitCodes.InvalidOption;
        }

        var hadErrors = false;
        foreach (var file in files)
        {
            try { deps.Load(file); }
            catch (Exception ex)
            {
                hadErrors = true;
                stderr.WriteLine(ex.Message);
            }
        }

        var formatter = deps.Resolve(args.Format, stdout);
        var result = Runner.RunTests(deps.Tests(), formatter).GetAwaiter().GetResult();

        if (Environment.GetEnvironmentVariable("DUCKTAPE_CHECK_SKIPPED") == "1" && result.Skipped > 0)
            return ExitCodes.Skipped;
        if (hadErrors || result.Failed > 0) return ExitCodes.Fail;
        return ExitCodes.Ok;
    }
}
