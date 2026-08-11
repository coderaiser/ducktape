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

    public void Write(string @event, dynamic data)
    {
        if (_ended) throw new InvalidOperationException("Harness received event after 'end'");

        string? result = @event switch
        {
            "start"        => _formatter.Start((int)data.total),
            "test"         => _formatter.Test((string)data.test),
            "test:end"     => _formatter.TestEnd((int)data.count, (int)data.total,
                                                  (int)data.failed, (string)data.test),
            "test:success" => _formatter.Success((int)data.count, (string)data.message),
            "test:fail"    => _formatter.Fail((string)data.at, (int)data.count,
                                               (string)data.message, (string)data.@operator,
                                               data.result, data.expected,
                                               (string)data.output, (string)data.error_stack),
            "comment"      => _formatter.Comment((string)data.message),
            "end"          => _formatter.End((int)data.count, (int)data.passed,
                                              (int)data.failed, (int)data.skipped),
            _              => null,
        };

        if (result is not null)
        {
            _stream.Write(result);
            _stream.Flush();
        }

        if (@event == "end") _ended = true;
    }
}
