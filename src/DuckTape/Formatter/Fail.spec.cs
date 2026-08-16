using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Test;

var test = CreateTest();

test("fail: test names are silent", t =>
{
    var sw = new StringWriter();
    var f = new FailFormatter(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test", new { test = "fail: hidden" });
    t.Equal(sw.ToString(), "TAP version 13\n");
    t.End();
});

test("fail: successes are silent", t =>
{
    var sw = new StringWriter();
    var f = new FailFormatter(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test:success", new { count = 1, message = "fail: passes" });
    t.Equal(sw.ToString(), "TAP version 13\n");
    t.End();
});

test("fail: failures are shown", t =>
{
    var sw = new StringWriter();
    var f = new FailFormatter(sw);
    f.Emit("test:fail", new { at = "a.cs:1", count = 1, message = "m", @operator = "equal", result = 1, expected = 2, output = "", error_stack = "st" });
    t.Ok(sw.ToString().Contains("not ok 1 m"));
    t.End();
});

test("fail: comments are inherited from tap", t =>
{
    var sw = new StringWriter();
    var f = new FailFormatter(sw);
    f.Emit("comment", new { message = "hi" });
    t.Ok(sw.ToString().Contains("# hi\n"));
    t.End();
});

test("fail: summary is inherited from tap", t =>
{
    var sw = new StringWriter();
    var f = new FailFormatter(sw);
    f.Emit("end", new { count = 1, passed = 1, failed = 0, skipped = 0 });
    t.Ok(sw.ToString().Contains("1..1"));
    t.End();
});

test("fail: test_end emits nothing extra", t =>
{
    var sw = new StringWriter();
    var f = new FailFormatter(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test:end", new { count = 1, total = 1, failed = 0, test = "x" });
    t.Equal(sw.ToString(), "TAP version 13\n");
    t.End();
});