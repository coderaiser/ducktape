namespace DuckTape.Formatter;

public class Harness
{
    readonly IFormatter _formatter;
    TextWriter _stream;
    bool _ended;

    public Harness(IFormatter formatter, TextWriter? stream = null)
    {
        _formatter = formatter;
        _stream = stream ?? Console.Out;
    }

    public void Pipe(TextWriter stream) => _stream = stream;

    public void Write(string @event, object? data)
    {
        if (_ended) throw new InvalidOperationException("Harness received event after 'end'");

        string? result = @event switch
        {
            "start"        => _formatter.Start(GetInt(data, "total")),
            "test"         => _formatter.Test(GetString(data, "test")),
            "test:end"     => _formatter.TestEnd(GetInt(data, "count"), GetInt(data, "total"),
                                                  GetInt(data, "failed"), GetString(data, "test")),
            "test:success" => _formatter.Success(GetInt(data, "count"), GetString(data, "message")),
            "test:fail"    => _formatter.Fail(GetString(data, "at"), GetInt(data, "count"),
                                               GetString(data, "message"), GetString(data, "operator"),
                                               Get(data, "result"), Get(data, "expected"),
                                               GetString(data, "output"), GetString(data, "error_stack")),
            "comment"      => _formatter.Comment(GetString(data, "message")),
            "end"          => _formatter.End(GetInt(data, "count"), GetInt(data, "passed"),
                                              GetInt(data, "failed"), GetInt(data, "skipped")),
            _              => null,
        };

        if (result is not null)
        {
            _stream.Write(result);
            _stream.Flush();
        }

        if (@event == "end") _ended = true;
    }

    static object? Get(object? data, string name) =>
        data?.GetType().GetProperty(name)?.GetValue(data);

    static int GetInt(object? data, string name) => Get(data, name) is int i ? i : 0;

    static string GetString(object? data, string name) => Get(data, name) as string ?? "";
}
