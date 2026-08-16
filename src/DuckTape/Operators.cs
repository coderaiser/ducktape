namespace DuckTape;

public record TestState(
    bool IsOk,
    string Message,
    object? Result = null,
    object? Expected = null,
    string Output = "",
    string Stack = "",
    string At = ""
);

public static class Operators
{
    public static TestState Ok(object? result, string message = "should be truthy") =>
        new(!IsFalsy(result), message, result, true);

    public static TestState NotOk(object? result, string message = "should be falsy") =>
        new(IsFalsy(result), message, result, false);

    static bool IsFalsy(object? result) =>
        result is null || result is false || result is 0 ||
        result is string s && s.Length == 0;

    public static TestState Equal<T>(T result, T expected, string message = "should equal")
    {
        var isOk = Equals(result, expected);
        return new(isOk, message, result, expected, isOk ? "" : Diff.MakeDiff(expected, result));
    }

    public static TestState NotEqual<T>(T result, T expected, string message = "should not equal") =>
        new(!Equals(result, expected), message, result, expected);

    public static TestState DeepEqual<T>(T result, T expected, string message = "should deep equal")
    {
        var ra = System.Text.Json.JsonSerializer.Serialize(result);
        var rb = System.Text.Json.JsonSerializer.Serialize(expected);
        var isOk = ra == rb;
        return new(isOk, message, result, expected, isOk ? "" : Diff.MakeDiff(expected, result));
    }

    public static TestState NotDeepEqual<T>(T result, T expected, string message = "should not deep equal")
    {
        var ra = System.Text.Json.JsonSerializer.Serialize(result);
        var rb = System.Text.Json.JsonSerializer.Serialize(expected);
        var isOk = ra != rb;
        return new(isOk, message, result, expected, isOk ? "" : Diff.MakeDiff(expected, result));
    }

    public static TestState Match(string result, string pattern, string message = "should match")
    {
        var isOk = System.Text.RegularExpressions.Regex.IsMatch(result, pattern);
        return new(isOk, message, result, pattern);
    }

    public static TestState NotMatch(string result, string pattern, string message = "should not match")
    {
        var state = Match(result, pattern, message);
        return state with { IsOk = !state.IsOk };
    }

    public static TestState Pass(string message = "(unnamed assert)") =>
        new(true, message);

    public static TestState Fail(Exception error, string at = "") =>
        new(false, error.Message, Stack: error.StackTrace ?? "", At: at);
}
