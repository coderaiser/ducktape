using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("entry: action overload runs its body", t =>
{
    t.Ok(true);
    t.End();
});

test("entry: createTest returns independent fn", t =>
{
    var test2 = CreateTest();
    t.Ok(test2 is not null);
    t.End();
});

test("entry: skip registers a skipped test", t =>
{
    Skip("entry: skipped registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});

test("entry: only registers an only test", t =>
{
    Only("entry: only registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});

var asyncTest = CreateTestAsync();

asyncTest("entry async overload runs its body", async t =>
{
    await Task.CompletedTask;
    t.Ok(true);
    t.End();
});