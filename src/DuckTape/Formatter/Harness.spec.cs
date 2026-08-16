using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Test;

var test = CreateTest();

test("harness: start routes to Start hook", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("start", new { total = 3 });
    t.DeepEqual(rec.Calls, new List<string> { "start:3" });
    t.End();
});

test("harness: test routes to Test hook", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("test", new { test = "name" });
    t.DeepEqual(rec.Calls, new List<string> { "test:name" });
    t.End();
});

test("harness: test_end routes with all args", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("test:end", new { count = 1, total = 2, failed = 0, test = "name" });
    t.DeepEqual(rec.Calls, new List<string> { "test:end:1:2:0" });
    t.End();
});

test("harness: test_success routes with message", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("test:success", new { count = 1, message = "passes" });
    t.DeepEqual(rec.Calls, new List<string> { "success:1:passes" });
    t.End();
});

test("harness: test_fail routes with all fields", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("test:fail", new { at = "a.cs", count = 1, message = "m", @operator = "equal", result = 1, expected = 2, output = "", error_stack = "st" });
    t.DeepEqual(rec.Calls, new List<string> { "fail:a.cs:1" });
    t.End();
});

test("harness: comment routes to Comment hook", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("comment", new { message = "hi" });
    t.DeepEqual(rec.Calls, new List<string> { "comment:hi" });
    t.End();
});

test("harness: end routes to End hook then locks", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("end", new { count = 1, passed = 1, failed = 0, skipped = 0 });
    var threw = false;
    try { h.Write("comment", new { message = "late" }); }
    catch (InvalidOperationException) { threw = true; }
    t.Ok(threw);
    t.End();
});

test("harness: unknown events are ignored", t =>
{
    var rec = new RecordingFormatter();
    var h = new Harness(rec, new StringWriter());
    h.Write("bogus", new { x = 1 });
    t.Equal(rec.Calls.Count, 0);
    t.End();
});

test("harness: null results are not written", t =>
{
    var sw = new StringWriter();
    var h = new Harness(new RecordingFormatter(), sw);
    h.Write("start", new { total = 1 });
    t.Equal(sw.ToString(), "");
    t.End();
});

test("harness: pipe redirects the stream", t =>
{
    var f = new EchoFormatter();
    var sw1 = new StringWriter();
    var sw2 = new StringWriter();
    var h = new Harness(f, sw1);
    h.Write("start", new { total = 1 });
    h.Pipe(sw2);
    h.Write("test", new { test = "n" });
    t.Ok(sw2.ToString().Contains("Y") && !sw1.ToString().Contains("Y"));
    t.End();
});

class RecordingFormatter : IFormatter
{
    public readonly List<string> Calls = new();
    public void Emit(string @event, object? data = null) { }
    public string? Start(int total) { Calls.Add("start:" + total); return null; }
    public string? Test(string test) { Calls.Add("test:" + test); return null; }
    public string? TestEnd(int count, int total, int failed, string test) { Calls.Add($"test:end:{count}:{total}:{failed}"); return null; }
    public string? Success(int count, string message) { Calls.Add($"success:{count}:{message}"); return null; }
    public string? Fail(string at, int count, string message, string @operator, object? result, object? expected, string output, string errorStack) { Calls.Add($"fail:{at}:{count}"); return null; }
    public string? Comment(string message) { Calls.Add("comment:" + message); return null; }
    public string? End(int count, int passed, int failed, int skipped) { Calls.Add("end"); return null; }
}

class EchoFormatter : IFormatter
{
    public void Emit(string @event, object? data = null) { }
    public string? Start(int total) => "X\n";
    public string? Test(string test) => "Y\n";
    public string? TestEnd(int count, int total, int failed, string test) => null;
    public string? Success(int count, string message) => null;
    public string? Fail(string at, int count, string message, string @operator, object? result, object? expected, string output, string errorStack) => null;
    public string? Comment(string message) => null;
    public string? End(int count, int passed, int failed, int skipped) => null;
}