using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Test;

var test = CreateTest();

List<TestDefinition> Mini(Action<T> fn) =>
    new() { new("t: mini", t2 => { fn(t2); return Task.CompletedTask; }) };

RunResult RunMini(List<TestDefinition> tests) =>
    Runner.RunTests(tests, new RecordingFormatter()).GetAwaiter().GetResult();

test("t: equal passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.Equal(1, 1); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: equal fail increments failed", t =>
{
    var r = RunMini(Mini(t2 => { t2.Equal(1, 2); t2.End(); }));
    t.Equal(r.Failed, 1);
    t.End();
    return Task.CompletedTask;
});

test("t: not_equal passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.NotEqual(1, 2); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: ok passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.Ok(true); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: not_ok passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.NotOk(0); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: deep_equal passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.DeepEqual(new[] { 1 }, new[] { 1 }); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: not_deep_equal passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.NotDeepEqual(new[] { 1, 2 }, new[] { 1, 3 }); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: not_deep_equal fails when equal", t =>
{
    var r = RunMini(Mini(t2 => { t2.NotDeepEqual(new[] { 1, 2 }, new[] { 1, 2 }); t2.End(); }));
    t.Equal(r.Failed, 1);
    t.End();
    return Task.CompletedTask;
});

test("t: match passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.Match("hello world", @"world"); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: not_match passes", t =>
{
    var r = RunMini(Mini(t2 => { t2.NotMatch("hello", @"world"); t2.End(); }));
    t.Equal(r.Failed, 0);
    t.End();
    return Task.CompletedTask;
});

test("t: pass assertion counts as pass", t =>
{
    var r = RunMini(Mini(t2 => { t2.Pass(); t2.End(); }));
    t.Equal(r.Passed, 1);
    t.End();
    return Task.CompletedTask;
});

test("t: fail assertion records exception", t =>
{
    var r = RunMini(Mini(t2 => { t2.Fail(new Exception("boom")); t2.End(); }));
    t.Equal(r.Failed, 1);
    t.End();
    return Task.CompletedTask;
});

test("t: comment emits a comment event", t =>
{
    var rec = new RecordingFormatter();
    var tests = new List<TestDefinition>
    {
        new("t: comment", t2 => { t2.Comment("hello"); t2.Ok(true); t2.End(); return Task.CompletedTask; }),
    };
    Runner.RunTests(tests, rec).GetAwaiter().GetResult();
    t.Ok(rec.Calls.Contains("comment"));
    t.End();
    return Task.CompletedTask;
});

test("t: assertions count toward the total", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", "0");
    var r = RunMini(Mini(t2 => { t2.Ok(true); t2.Ok(true); t2.End(); }));
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", null);
    t.Equal(r.Passed, 2);
    t.End();
    return Task.CompletedTask;
});

test("t: doubling end fails once", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", "0");
    var r = RunMini(Mini(t2 => { t2.Ok(true); t2.End(); t2.End(); }));
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", null);
    t.Equal(r.Failed, 1);
    t.End();
    return Task.CompletedTask;
});

test("t: asserting after end fails once", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", "0");
    var r = RunMini(Mini(t2 => { t2.Ok(true); t2.End(); t2.Ok(false); }));
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", null);
    t.Equal(r.Failed, 1);
    t.End();
    return Task.CompletedTask;
});

test("t: triple end only fails once", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", "0");
    var r = RunMini(Mini(t2 => { t2.Ok(true); t2.End(); t2.End(); t2.End(); }));
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", null);
    t.Equal(r.Failed, 1);
    t.End();
    return Task.CompletedTask;
});

class RecordingFormatter : IFormatter
{
    public readonly List<string> Calls = new();
    public void Emit(string @event, object? data = null) => Calls.Add(@event);
    public string? Start(int total) { Calls.Add("start"); return null; }
    public string? Test(string test) { Calls.Add("test"); return null; }
    public string? TestEnd(int count, int total, int failed, string test) { Calls.Add("test:end"); return null; }
    public string? Success(int count, string message) { Calls.Add("success"); return null; }
    public string? Fail(string at, int count, string message, string @operator, object? result, object? expected, string output, string errorStack) { Calls.Add("fail"); return null; }
    public string? Comment(string message) { Calls.Add("comment"); return null; }
    public string? End(int count, int passed, int failed, int skipped) { Calls.Add("end"); return null; }
}