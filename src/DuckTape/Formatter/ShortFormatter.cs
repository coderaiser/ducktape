namespace DuckTape.Formatter;

/// <summary>Compact supertape-style output: one short summary line per test, failures with details.</summary>
public class ShortFormatter : FormatterBase
{
    public ShortFormatter(TextWriter? stream = null) : base(stream) { }

    public override string? Test(string test) => $">{test}\n";
    public override string? Comment(string message) => $"# {message}\n";

    public override string? Success(int count, string message) => $"OK {count} {message}\n";

    public override string? Fail(string at, int count, string message, string @operator,
                                 object? result, object? expected, string output, string errorStack)
    {
        var lines = new List<string> { $"NOT OK {count} {message}" };
        if (!string.IsNullOrEmpty(output))
            lines.Add(output);
        else
        {
            lines.Add($"  operator: {@operator}");
            lines.Add($"  expected: {expected}");
            lines.Add($"  result:   {result}");
        }
        if (!string.IsNullOrEmpty(at)) lines.Add($"  at: {at}");
        lines.Add($"  stack: {errorStack}");
        return string.Join('\n', lines) + "\n";
    }

    public override string? End(int count, int passed, int failed, int skipped)
    {
        var lines = new List<string> { "" };
        lines.Add($"# tests {count}");
        lines.Add($"# pass {passed}");
        if (skipped > 0) lines.Add($"# skip {skipped}");
        if (failed > 0) lines.Add($"# fail {failed}");
        lines.Add(failed == 0 ? "# ok" : "");
        return string.Join('\n', lines);
    }
}
