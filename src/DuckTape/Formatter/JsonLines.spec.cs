using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Tests;

Test("json_lines: start emits one json object", t =>
{
    var sw = new StringWriter();
    var f = new JsonLinesFormatter(sw);
    f.Emit("start", new { total = 2 });
    t.Ok(sw.ToString().Contains("{\"type\":\"start\",\"total\":2}"));
    t.End();
});

Test("json_lines: test is silent", t =>
{
    var sw = new StringWriter();
    var f = new JsonLinesFormatter(sw);
    f.Emit("test", new { test = "x" });
    t.Equal(sw.ToString(), "");
    t.End();
});

Test("json_lines: test_end emits progress json", t =>
{
    var sw = new StringWriter();
    var f = new JsonLinesFormatter(sw);
    f.Emit("test:end", new { count = 1, total = 2, failed = 0, test = "jl: sample" });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("\"type\":\"test:end\"") && out_.Contains("jl: sample"));
    t.End();
});

Test("json_lines: success is silent", t =>
{
    var sw = new StringWriter();
    var f = new JsonLinesFormatter(sw);
    f.Emit("test:success", new { count = 1, message = "m" });
    t.Equal(sw.ToString(), "");
    t.End();
});

Test("json_lines: fail emits failure json", t =>
{
    var sw = new StringWriter();
    var f = new JsonLinesFormatter(sw);
    f.Emit("test:fail", new { at = "a.cs:1", count = 1, message = "boom", @operator = "equal", result = 1, expected = 2, output = "", error_stack = "st" });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("\"type\":\"fail\"") && out_.Contains("boom"));
    t.End();
});

Test("json_lines: comment emits comment json", t =>
{
    var sw = new StringWriter();
    var f = new JsonLinesFormatter(sw);
    f.Emit("comment", new { message = "hi" });
    t.Ok(sw.ToString().Contains("\"type\":\"comment\""));
    t.End();
});

Test("json_lines: end emits summary json", t =>
{
    var sw = new StringWriter();
    var f = new JsonLinesFormatter(sw);
    f.Emit("end", new { count = 1, passed = 1, failed = 0, skipped = 0 });
    t.Ok(sw.ToString().Contains("\"type\":\"end\""));
    t.End();
});