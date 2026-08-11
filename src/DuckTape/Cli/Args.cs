namespace DuckTape.Cli;

public class InvalidFormatException : Exception
{
    public InvalidFormatException(string message) : base(message) { }
}

public record Args(
    string Format,
    bool NoWorker,
    bool NoCheckDuplicates,
    bool NoCheckAssertionsCount,
    bool Help,
    bool Version,
    string[] Patterns
);

public static class ArgParser
{
    static readonly string[] ValidFormats = { "tap", "fail", "short", "progress-bar", "json-lines" };

    public static Args Parse(string[] argv)
    {
        var format = Environment.GetEnvironmentVariable("CI") == "1" ? "tap" : "progress-bar";
        bool noWorker = false, noDupes = false, noCount = false, help = false, version = false;
        var patterns = new List<string>();

        for (int i = 0; i < argv.Length; i++)
        {
            switch (argv[i])
            {
                case "-h": case "--help":                    help = true;    break;
                case "-v": case "--version":                 version = true; break;
                case "--no-worker":                          noWorker = true; break;
                case "--no-check-duplicates":                noDupes  = true; break;
                case "--no-check-assertions-count":          noCount  = true; break;
                case "-f": case "--format":
                    if (i + 1 >= argv.Length)
                        throw new InvalidFormatException("ducktape: --format requires a value");
                    format = argv[++i];
                    if (!ValidFormats.Contains(format))
                        throw new InvalidFormatException($"ducktape: unknown format '{format}'");
                    break;
                default:
                    patterns.Add(argv[i]);
                    break;
            }
        }

        return new(format, noWorker, noDupes, noCount, help, version, patterns.ToArray());
    }
}
