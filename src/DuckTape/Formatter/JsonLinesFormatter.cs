using System.Text.Json;

namespace DuckTape.Formatter;

/// <summary>One JSON object per line. TestEnd emits {count,total,failed,test}; Fail emits a failure object; Success returns null.</summary>
public class JsonLinesFormatter : FormatterBase
{
    public JsonLinesFormatter(TextWriter? stream = null) : base(stream) { }

    public override string? Start(int total) => $"{{\"type\":\"start\",\"total\":{total}}}\n";
    public override string? Test(string test) => null;

    public override string? TestEnd(int count, int total, int failed, string test) =>
        JsonSerializer.Serialize(new { type = "test:end", count, total, failed, test }) + "\n";

    public override string? Success(int count, string message) => null;

    public override string? Fail(string at, int count, string message, string @operator,
                                 object? result, object? expected, string output, string errorStack)
    {
        var payload = JsonSerializer.Serialize(new
        {
            type = "fail", at, count, message, @operator, result, expected, output, error_stack = errorStack,
        });
        return payload + "\n";
    }

    public override string? Comment(string message) =>
        JsonSerializer.Serialize(new { type = "comment", message }) + "\n";

    public override string? End(int count, int passed, int failed, int skipped) =>
        JsonSerializer.Serialize(new { type = "end", count, passed, failed, skipped }) + "\n";
}
