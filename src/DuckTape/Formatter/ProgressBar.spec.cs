using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Test;

var test = CreateTest();

ProgressBarFormatter Bar(StringWriter sw)
{
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR", "1");
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_MIN", "1");
    Environment.SetEnvironmentVariable("CI", null);
    return new ProgressBarFormatter(sw);
}

test("progress_bar: forced bar starts at zero percent", t =>
{
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 2 });
    t.Ok(sw.ToString().Contains("\r[") && sw.ToString().Contains("0%"));
    t.End();
});

test("progress_bar: success advances the bar", t =>
{
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 2 });
    f.Emit("test:success", new { count = 1, message = "pb: one" });
    t.Ok(sw.ToString().Contains("50%"));
    t.End();
});

test("progress_bar: fail shows stack when enabled", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_STACK", "1");
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test:fail", new { at = "a.cs:1", count = 1, message = "pb: bad", @operator = "op", result = 1, expected = 2, output = "", error_stack = "st" });
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_STACK", null);
    t.Ok(sw.ToString().Contains("pb: bad") && sw.ToString().Contains("a.cs:1"));
    t.End();
});

test("progress_bar: fail hides stack when disabled", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_STACK", "0");
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test:fail", new { at = "hidden.cs:1", count = 1, message = "pb: bad", @operator = "op", result = 1, expected = 2, output = "", error_stack = "st" });
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_STACK", null);
    t.Ok(!sw.ToString().Contains("hidden.cs:1"));
    t.End();
});

test("progress_bar: end summarizes the run", t =>
{
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test:success", new { count = 1, message = "pb: pass" });
    f.Emit("end", new { count = 1, passed = 1, failed = 0, skipped = 0 });
    var out_ = sw.ToString();
    t.Ok(out_.Contains("# pass 1") && out_.Contains("# fail 0"));
    t.End();
});

test("progress_bar: zero total renders safely", t =>
{
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 0 });
    t.Ok(sw.ToString().Contains("0%"));
    t.End();
});

test("progress_bar: test and comment are silent in bar mode", t =>
{
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 1 });
    var before = sw.ToString().Length;
    f.Emit("test", new { test = "pb: hidden" });
    f.Emit("comment", new { message = "pb: hidden" });
    t.Equal(sw.ToString().Length, before);
    t.End();
});

test("progress_bar: ci forces tap fallback", t =>
{
    Environment.SetEnvironmentVariable("CI", "1");
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR", null);
    var sw = new StringWriter();
    var f = new ProgressBarFormatter(sw);
    f.Emit("start", new { total = 1 });
    Environment.SetEnvironmentVariable("CI", null);
    t.Ok(sw.ToString().Contains("TAP version 13"));
    t.End();
});

test("progress_bar: force off falls back to tap", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR", "0");
    Environment.SetEnvironmentVariable("CI", null);
    var sw = new StringWriter();
    var f = new ProgressBarFormatter(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test:success", new { count = 1, message = "pb: tap" });
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR", null);
    t.Ok(sw.ToString().Contains("ok 1 pb: tap"));
    t.End();
});

test("progress_bar: below minimum falls back to tap", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR", null);
    Environment.SetEnvironmentVariable("CI", null);
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_MIN", "999");
    var sw = new StringWriter();
    var f = new ProgressBarFormatter(sw);
    f.Emit("start", new { total = 1 });
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_MIN", null);
    t.Ok(sw.ToString().Contains("TAP version 13"));
    t.End();
});

test("progress_bar: bar allowed when total meets minimum", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR", null);
    Environment.SetEnvironmentVariable("CI", null);
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_MIN", "1");
    var sw = new StringWriter();
    var f = new ProgressBarFormatter(sw);
    f.Emit("start", new { total = 1 });
    Environment.SetEnvironmentVariable("DUCKTAPE_PROGRESS_BAR_MIN", null);
    t.Ok(sw.ToString().Contains("[") && sw.ToString().Contains("0%") && !sw.ToString().Contains("TAP"));
    t.End();
});

test("progress_bar: test_end is silent in bar mode", t =>
{
    var sw = new StringWriter();
    var f = Bar(sw);
    f.Emit("start", new { total = 1 });
    var before = sw.ToString().Length;
    f.Emit("test:end", new { count = 1, total = 1, failed = 0, test = "pb: end" });
    t.Equal(sw.ToString().Length, before);
    t.End();
});