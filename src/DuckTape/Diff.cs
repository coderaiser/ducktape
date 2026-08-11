namespace DuckTape;

public static class Diff
{
    public static string MakeDiff(object? expected, object? result)
    {
        var a = Format(expected);
        var b = Format(result);
        if (a == b) return string.Empty;

        return $"""
              diff: |-
                - {a}
                + {b}
            """;
    }

    static string Format(object? value) =>
        value is null ? "null" : System.Text.Json.JsonSerializer.Serialize(value);
}
