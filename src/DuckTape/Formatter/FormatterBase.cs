namespace DuckTape.Formatter;

public abstract class FormatterBase : IFormatter
{
    readonly Harness _harness;

    protected FormatterBase(TextWriter? stream = null) => _harness = new(this, stream);

    public void Emit(string @event, object? data = null) => _harness.Write(@event, data!);

    public virtual string? Start(int total) => null;
    public virtual string? Test(string test) => null;
    public virtual string? TestEnd(int count, int total, int failed, string test) => null;
    public virtual string? Success(int count, string message) => null;
    public virtual string? Fail(string at, int count, string message, string @operator,
                                object? result, object? expected, string output, string errorStack) => null;
    public virtual string? Comment(string message) => null;
    public virtual string? End(int count, int passed, int failed, int skipped) => null;
}
