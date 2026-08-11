using DuckTape;
using static DuckTape.Test;

Run("entry: action overload runs its body", t =>
{
    t.Ok(true);
    t.End();
});

Run("entry: func overload runs its body", async t =>
{
    await Task.CompletedTask;
    t.Ok(true);
    t.End();
});

Run("entry: skip registers a skipped test", t =>
{
    Skip("entry: skipped registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});

Run("entry: only registers an only test", t =>
{
    Only("entry: only registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});

Run("entry: named skip param registers", t =>
{
    Run("entry: named skipped registration", t2 => Task.CompletedTask, skip: true);
    t.Ok(true);
    t.End();
});

Run("entry: named only param registers", t =>
{
    Run("entry: named only registration", t2 => { t2.Ok(true); t2.End(); }, only: true);
    t.Ok(true);
    t.End();
});