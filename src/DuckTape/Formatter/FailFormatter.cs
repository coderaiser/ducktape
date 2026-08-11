namespace DuckTape.Formatter;

/// <summary>Shows only failures. Passes are silent. Start, Comment, End are inherited from Tap.</summary>
public class FailFormatter : TapFormatter
{
    public FailFormatter(TextWriter? stream = null) : base(stream) { }

    public override string? Test(string test) => null;
    public override string? Success(int count, string message) => null;
    public override string? TestEnd(int count, int total, int failed, string test) => null;
}
