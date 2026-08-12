using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Tests;

Test("formatter_resolver: resolves tap", t =>
{
    t.Ok(FormatterResolver.Resolve("tap") is TapFormatter);
    t.End();
});

Test("formatter_resolver: resolves fail", t =>
{
    t.Ok(FormatterResolver.Resolve("fail") is FailFormatter);
    t.End();
});

Test("formatter_resolver: resolves short", t =>
{
    t.Ok(FormatterResolver.Resolve("short") is ShortFormatter);
    t.End();
});

Test("formatter_resolver: resolves json-lines", t =>
{
    t.Ok(FormatterResolver.Resolve("json-lines") is JsonLinesFormatter);
    t.End();
});

Test("formatter_resolver: resolves progress-bar", t =>
{
    t.Ok(FormatterResolver.Resolve("progress-bar") is ProgressBarFormatter);
    t.End();
});

Test("formatter_resolver: rejects unknown format", t =>
{
    var threw = false;
    try { FormatterResolver.Resolve("nope"); }
    catch (ArgumentException ex) { threw = ex.Message.Contains("nope"); }
    t.Ok(threw);
    t.End();
});

Test("formatter_resolver: honors the stream", t =>
{
    var sw = new StringWriter();
    FormatterResolver.Resolve("tap", sw).Emit("start", new { total = 1 });
    t.Ok(sw.ToString().Contains("TAP version 13"));
    t.End();
});

Test("formatter_base: bare formatter returns null for every hook", t =>
{
    var sw = new StringWriter();
    var f = new BareFormatter(sw);
    f.Emit("start", new { total = 1 });
    f.Emit("test", new { test = "x" });
    f.Emit("test:end", new { count = 1, total = 1, failed = 0, test = "x" });
    f.Emit("test:success", new { count = 1, message = "m" });
    f.Emit("test:fail", new { at = "a", count = 1, message = "m", @operator = "op", result = 1, expected = 2, output = "", error_stack = "" });
    f.Emit("comment", new { message = "c" });
    f.Emit("end", new { count = 1, passed = 1, failed = 0, skipped = 0 });
    t.Equal(sw.ToString(), "");
    t.End();
});

Test("formatter_base: default stream is console", t =>
{
    t.Ok(new BareFormatter() is IFormatter);
    t.End();
});

class BareFormatter : FormatterBase
{
    public BareFormatter(TextWriter? stream = null) : base(stream) { }
}