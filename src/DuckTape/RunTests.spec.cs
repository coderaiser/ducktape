using DuckTape;
using DuckTape.Formatter;
using static DuckTape.Tests;

RunResult RunMini(List<TestDefinition> tests) =>
    Runner.RunTests(tests, new RecordingFormatter()).GetAwaiter().GetResult();

Test("run_tests: runs tests serially", t =>
{
    var order = new List<string>();
    var tests = new List<TestDefinition>
    {
        new("scope: a", t => { order.Add("a"); t.Ok(true); t.End(); return Task.CompletedTask; }),
        new("scope: b", t => { order.Add("b"); t.Ok(true); t.End(); return Task.CompletedTask; }),
    };
    RunMini(tests);
    t.DeepEqual(order, new List<string> { "a", "b" });
    t.End();
});

Test("run_tests: skipped tests do not run", t =>
{
    var ran = new List<int>();
    var tests = new List<TestDefinition>
    {
        new("scope: s", t => { ran.Add(1); t.Ok(true); t.End(); return Task.CompletedTask; }, Skip: true),
    };
    RunMini(tests);
    t.Equal(ran.Count, 0);
    t.End();
});

Test("run_tests: only tests skip others", t =>
{
    var ran = new List<string>();
    var tests = new List<TestDefinition>
    {
        new("scope: only", t => { ran.Add("only"); t.Ok(true); t.End(); return Task.CompletedTask; }, Only: true),
        new("scope: other", t => { ran.Add("other"); t.Ok(true); t.End(); return Task.CompletedTask; }),
    };
    RunMini(tests);
    t.DeepEqual(ran, new List<string> { "only" });
    t.End();
});

Test("run_tests: result holds counts", t =>
{
    var tests = new List<TestDefinition>
    {
        new("scope: one", t => { t.Ok(true); t.End(); return Task.CompletedTask; }),
        new("scope: two", t => { t.Ok(true); t.End(); return Task.CompletedTask; }),
    };
    var r = RunMini(tests);
    t.DeepEqual(new[] { r.Count, r.Passed, r.Failed, r.Skipped }, new[] { 2, 2, 0, 0 });
    t.End();
});

Test("run_tests: exception in test body fails the test", t =>
{
    var tests = new List<TestDefinition>
    {
        new("scope: throws", t2 => { t2.Ok(true); throw new InvalidOperationException("oops"); }),
    };
    var r = RunMini(tests);
    t.Equal(r.Failed, 1);
    t.End();
});

Test("run_tests: slow test times out", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_TIMEOUT", "50");
    var tests = new List<TestDefinition>
    {
        new("scope: slow", async t2 => { t2.Ok(true); await Task.Delay(1000); t2.End(); }),
    };
    var r = RunMini(tests);
    Environment.SetEnvironmentVariable("DUCKTAPE_TIMEOUT", null);
    t.Equal(r.Failed, 1);
    t.End();
});

Test("run_tests: zero assertions fail validation", t =>
{
    var tests = new List<TestDefinition>
    {
        new("scope: none", _ => Task.CompletedTask),
    };
    var r = RunMini(tests);
    t.Equal(r.Failed, 1);
    t.End();
});

Test("run_tests: extra assertions fail validation", t =>
{
    var tests = new List<TestDefinition>
    {
        new("scope: extra", t2 => { t2.Ok(true); t2.Ok(true); t2.End(); return Task.CompletedTask; }),
    };
    var r = RunMini(tests);
    t.Equal(r.Failed, 1);
    t.End();
});

Test("run_tests: start and end events are emitted", t =>
{
    var rec = new RecordingFormatter();
    var tests = new List<TestDefinition>
    {
        new("scope: ab", t2 => { t2.Ok(true); t2.End(); return Task.CompletedTask; }),
    };
    Runner.RunTests(tests, rec).GetAwaiter().GetResult();
    t.Ok(rec.Calls.Contains("start") && rec.Calls.Contains("end"));
    t.End();
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