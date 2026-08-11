namespace DuckTape.Formatter;

public static class FormatterResolver
{
    public static IFormatter Resolve(string format, TextWriter? stream = null) => format switch
    {
        "tap"          => new TapFormatter(stream),
        "fail"         => new FailFormatter(stream),
        "short"        => new ShortFormatter(stream),
        "json-lines"   => new JsonLinesFormatter(stream),
        "progress-bar" => new ProgressBarFormatter(stream ?? Console.Error),
        _              => throw new ArgumentException($"ducktape: unknown format '{format}'", nameof(format)),
    };
}
