using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Test;

var test = CreateTest();

test("tap: start prints the tap header", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("start", new { total = 1 });
    t.Ok(sw.ToString().Contains("TAP version 13\n"));
    t.End();
    return Task.CompletedTask;
});

test("tap: test prints a comment header", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("test", new { test = "tap: sample" });
    t.Ok(sw.ToString().Contains("# tap: sample\n"));
    t.End();
    return Task.CompletedTask;
});

test("tap: success prints an ok line", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test:success", new { count = 1, message = "tap: passes" });
    t.Ok(sw.ToString().Contains("ok 1 tap: passes\n"));
    t.End();
    return Task.CompletedTask;
});

test("tap: comment prints a comment line", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("comment", new { message = "hi" });
    t.Ok(sw.ToString().Contains("# hi\n"));
    t.End();
    return Task.CompletedTask;
});

test("tap: fail without output prints expected and result and stack", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("test:fail", new { at = "a.cs:1", count = 1, message = "m", @operator = "equal", result = 1, expected = 2, output = "", error_stack = "st" });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("expected: |-") && out_.Contains("result: |-") && out_.Contains("stack: |-"));
    t.End();
    return Task.CompletedTask;
});

test("tap: fail with output prints the output", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("test:fail", new { at = "a.cs:1", count = 1, message = "m", @operator = "equal", result = 1, expected = 2, output = "      obj: 7", error_stack = "st" });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("obj: 7") && !out_.Contains("expected: |-"));
    t.End();
    return Task.CompletedTask;
});

test("tap: fail includes the location", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("test:fail", new { at = "loc.cs:9", count = 1, message = "m", @operator = "op", result = 1, expected = 2, output = "", error_stack = "" });
    t.Ok(sw.ToString().Contains("loc.cs:9"));
    t.End();
    return Task.CompletedTask;
});

test("tap: end prints the plan and ok", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("end", new { count = 1, passed = 1, failed = 0, skipped = 0 });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("1..1") && out_.Contains("# tests 1") && out_.Contains("# ok"));
    t.End();
    return Task.CompletedTask;
});

test("tap: end includes skip line", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("end", new { count = 1, passed = 0, failed = 0, skipped = 1 });
    t.Ok(sw.ToString().Contains("# skip 1"));
    t.End();
    return Task.CompletedTask;
});

test("tap: end includes fail line without ok", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("end", new { count = 1, passed = 0, failed = 1, skipped = 0 });
    t.Ok(sw.ToString().Contains("# fail 1") && !sw.ToString().Contains("# ok"));
    t.End();
    return Task.CompletedTask;
});

test("tap: test_end produces no output", t =>
{
    var sw = new StringWriter();
    var f = new TapFormatter(sw);
    f.Emit("test:end", new { count = 1, total = 1, failed = 0, test = "x" });
    t.Equal(sw.ToString(), "");
    t.End();
    return Task.CompletedTask;
});