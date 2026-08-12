using DuckTape;
using static DuckTape.Tests;

Test("entry: action overload runs its body", t =>
{
    t.Ok(true);
    t.End();
});

Test("entry: func overload runs its body", async t =>
{
    await Task.CompletedTask;
    t.Ok(true);
    t.End();
});

Test("entry: skip registers a skipped test", t =>
{
    Skip("entry: skipped registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});

Test("entry: only registers an only test", t =>
{
    Only("entry: only registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});

Test("entry: named skip param registers", t =>
{
    Test("entry: named skipped registration", t2 => Task.CompletedTask, skip: true);
    t.Ok(true);
    t.End();
});

Test("entry: named only param registers", t =>
{
    Test("entry: named only registration", t2 => { t2.Ok(true); t2.End(); }, only: true);
    t.Ok(true);
    t.End();
});