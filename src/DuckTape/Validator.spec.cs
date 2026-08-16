using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

List<TestDefinition> MakeTests(string msg) =>
    new() { new(msg, _ => Task.CompletedTask) };

test("validator: passes when one assertion", t =>
{
    var tests = MakeTests("scope: subject");
    var v = new Validator(tests);
    t.Equal(v.Validate("scope: subject", 1).Message, null);
    t.End();
    return Task.CompletedTask;
});

test("validator: fails when zero assertions", t =>
{
    var tests = MakeTests("scope: zero");
    var v = new Validator(tests);
    t.Ok(v.Validate("scope: zero", 0).Message is not null);
    t.End();
    return Task.CompletedTask;
});

test("validator: fails when more than one assertion", t =>
{
    var tests = MakeTests("scope: many");
    var v = new Validator(tests);
    t.Ok(v.Validate("scope: many", 2).Message is not null);
    t.End();
    return Task.CompletedTask;
});

test("validator: duplicate message is reported", t =>
{
    var tests = new List<TestDefinition>
    {
        new("scope: dup", _ => Task.CompletedTask),
        new("scope: dup", _ => Task.CompletedTask),
    };
    var v = new Validator(tests);
    t.Ok(v.Validate("scope: dup", 1).Message is not null);
    t.End();
    return Task.CompletedTask;
});

test("validator: duplicate reported only once", t =>
{
    var tests = new List<TestDefinition>
    {
        new("scope: duponce", _ => Task.CompletedTask),
        new("scope: duponce", _ => Task.CompletedTask),
    };
    var v = new Validator(tests);
    v.Validate("scope: duponce", 1);
    t.Equal(v.Validate("scope: duponce", 1).Message, null);
    t.End();
    return Task.CompletedTask;
});

test("validator: single message is not duplicate", t =>
{
    var tests = MakeTests("scope: single");
    var v = new Validator(tests);
    t.Equal(v.Validate("scope: single", 1).Message, null);
    t.End();
    return Task.CompletedTask;
});

test("validator: duplicate check can be disabled", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_DUPLICATES", "0");
    var tests = new List<TestDefinition>
    {
        new("scope: offd", _ => Task.CompletedTask),
        new("scope: offd", _ => Task.CompletedTask),
    };
    var v = new Validator(tests);
    var msg = v.Validate("scope: offd", 1).Message;
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_DUPLICATES", null);
    t.Equal(msg, null);
    t.End();
    return Task.CompletedTask;
});

test("validator: assertion count check can be disabled", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", "0");
    var tests = MakeTests("scope: offn");
    var v = new Validator(tests);
    var msg = v.Validate("scope: offn", 0).Message;
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", null);
    t.Equal(msg, null);
    t.End();
    return Task.CompletedTask;
});

test("validator: scope check passes for valid format", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES", "1");
    var v = new Validator(MakeTests("scope: good"));
    var msg = v.Validate("scope: good", 1).Message;
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES", null);
    t.Equal(msg, null);
    t.End();
    return Task.CompletedTask;
});

test("validator: scope check fails without a colon", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES", "1");
    var v = new Validator(MakeTests("no scope here"));
    var msg = v.Validate("no scope here", 1).Message;
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES", null);
    t.Ok(msg is not null && msg.Contains("Scope required"));
    t.End();
    return Task.CompletedTask;
});

test("validator: scope check is off by default", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES", null);
    var v = new Validator(MakeTests("off by default"));
    var msg = v.Validate("off by default", 1).Message;
    t.Equal(msg, null);
    t.End();
    return Task.CompletedTask;
});

test("validator: error carries first duplicate location", t =>
{
    var tests = new List<TestDefinition>
    {
        new("scope: at", _ => Task.CompletedTask, At: "a.cs:1"),
        new("scope: at", _ => Task.CompletedTask, At: "b.cs:2"),
    };
    var v = new Validator(tests);
    var result = v.Validate("scope: at", 1);
    t.Equal(result.At, "b.cs:2");
    t.End();
    return Task.CompletedTask;
});