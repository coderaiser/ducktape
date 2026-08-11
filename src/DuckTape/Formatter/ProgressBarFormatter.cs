namespace DuckTape.Formatter;

/// <summary>
/// Renders an animated bar. Falls back to TAP output when CI=true, when the
/// progress bar is disabled, or when total is below DUCKTAPE_PROGRESS_BAR_MIN.
/// Controlled by DUCKTAPE_PROGRESS_BAR (1 force on, 0 force off) and
/// DUCKTAPE_PROGRESS_BAR_STACK (1 show stack on failures).
/// </summary>
public class ProgressBarFormatter : FormatterBase
{
    readonly TapFormatter _tap;
    bool _bar;
    int _total;
    int _done;
    int _failed;

    public ProgressBarFormatter(TextWriter? stream = null) : base(stream)
    {
        _tap = new TapFormatter(stream);
    }

    bool BarAllowed(int total)
    {
        var force = Environment.GetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR");
        if (force == "0") return false;
        if (Environment.GetEnvironmentVariable("CI") == "1") return false;
        var min = int.TryParse(Environment.GetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_MIN"), out var m) ? m : 100;
        if (total < min) return false;
        return true;
    }

    public override string? Start(int total)
    {
        _total = total;
        _bar = BarAllowed(total);
        return _bar ? Bar() : _tap.Start(total);
    }

    public override string? Test(string test) => _bar ? null : _tap.Test(test);
    public override string? TestEnd(int count, int total, int failed, string test) => null;
    public override string? Comment(string message) => _bar ? null : _tap.Comment(message);

    public override string? Success(int count, string message)
    {
        if (!_bar) return _tap.Success(count, message);
        _done = count;
        return Bar();
    }

    public override string? Fail(string at, int count, string message, string @operator,
                                 object? result, object? expected, string output, string errorStack)
    {
        if (!_bar) return _tap.Fail(at, count, message, @operator, result, expected, output, errorStack);
        _done = count;
        _failed++;
        var extra = "";
        if (Environment.GetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_STACK") == "1")
            extra = $"\n  {message}\n  {at}\n  {errorStack}";
        return Bar() + extra;
    }

    public override string? End(int count, int passed, int failed, int skipped)
    {
        if (!_bar) return _tap.End(count, passed, failed, skipped);
        return $"\n# tests {count}\n# pass {passed}\n# fail {failed}\n";
    }

    string Bar()
    {
        var width = 40;
        if (_total <= 0) return "\r[                    ] 0%";
        var filled = (int)((double)_done / _total * width);
        var bar = new string('=', filled).PadRight(width);
        var pct = (int)((double)_done / _total * 100);
        return $"\r[{bar}] {pct}%";
    }
}
