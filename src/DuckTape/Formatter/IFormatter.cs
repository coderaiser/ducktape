namespace DuckTape.Formatter;

public interface IFormatter
{
    void Emit(string @event, object? data = null);

    string? Start(int total);
    string? Test(string test);
    string? TestEnd(int count, int total, int failed, string test);
    string? Success(int count, string message);
    string? Fail(string at, int count, string message, string @operator,
                 object? result, object? expected, string output, string errorStack);
    string? Comment(string message);
    string? End(int count, int passed, int failed, int skipped);
}
