namespace DuckTape.Formatter;

public class TapFormatter : FormatterBase
{
    public TapFormatter(TextWriter? stream = null) : base(stream) { }

    public override string? Start(int total) => "TAP version 13\n";
    public override string? Test(string test) => $"# {test}\n";
    public override string? TestEnd(int count, int total, int failed, string test) => null;
    public override string? Comment(string message) => $"# {message}\n";
    public override string? Success(int count, string message) => $"ok {count} {message}\n";

    public override string? Fail(string at, int count, string message, string @operator,
                                 object? result, object? expected, string output, string errorStack)
    {
        var lines = new List<string>
        {
            $"not ok {count} {message}", "  ---", $"    operator: {@operator}",
        };
        if (!string.IsNullOrEmpty(output))
            lines.Add(output);
        else
            lines.AddRange(new[] { "    expected: |-", $"      {expected}", "    result: |-", $"      {result}" });
        lines.Add($"    {at}");
        lines.AddRange(new[] { "    stack: |-", errorStack, "  ...", "" });
        return string.Join('\n', lines) + "\n";
    }

    public override string? End(int count, int passed, int failed, int skipped)
    {
        var lines = new List<string> { "", $"1..{count}", $"# tests {count}", $"# pass {passed}" };
        if (skipped > 0) lines.Add($"# skip {skipped}");
        if (failed > 0) lines.Add($"# fail {failed}");
        lines.Add("");
        if (failed == 0) lines.Add("# ok");
        lines.Add("");
        return string.Join('\n', lines);
    }
}
