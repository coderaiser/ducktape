using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Tests;

Test("short: start is silent", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("start", new { total = 1 });
    t.Equal(sw.ToString(), "");
    t.End();
});

Test("short: test prints a chevron line", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("test", new { test = "short: sample" });
    t.Ok(sw.ToString().Contains(">short: sample"));
    t.End();
});

Test("short: success prints an ok summary", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("test:success", new { count = 1, message = "short: passes" });
    t.Ok(sw.ToString().Contains("OK 1 short: passes"));
    t.End();
});

Test("short: comment prints a comment line", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("comment", new { message = "hi" });
    t.Ok(sw.ToString().Contains("# hi"));
    t.End();
});

Test("short: fail prints a detailed block", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("test:fail", new { at = "a.cs:1", count = 1, message = "m", @operator = "equal", result = 1, expected = 2, output = "", error_stack = "st" });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("NOT OK 1 m") && out_.Contains("expected: 2") && out_.Contains("result:   1"));
    t.End();
});

Test("short: fail includes location and stack", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("test:fail", new { at = "loc.cs:9", count = 1, message = "m", @operator = "op", result = 1, expected = 2, output = "", error_stack = "trace" });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("at: loc.cs:9") && out_.Contains("stack: trace"));
    t.End();
});

Test("short: fail prints output when present", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("test:fail", new { at = "", count = 1, message = "m", @operator = "op", result = 1, expected = 2, output = "      obj: 7", error_stack = "" });
    t.Ok(sw.ToString().Contains("obj: 7"));
    t.End();
});

Test("short: end prints a passing summary", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("end", new { count = 2, passed = 2, failed = 0, skipped = 0 });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("# tests 2") && out_.Contains("# pass 2") && out_.Contains("# ok"));
    t.End();
});

Test("short: end prints fail and skip lines", t =>
{
    var sw = new StringWriter();
    var f = new ShortFormatter(sw);
    f.Emit("end", new { count = 3, passed = 1, failed = 1, skipped = 1 });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("# fail 1") && out_.Contains("# skip 1"));
    t.End();
});