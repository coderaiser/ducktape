using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("worker: runs an action", t =>
{
    var list = new List<int>();
    Worker.Run(() => list.Add(1));
    t.Equal(list.Count, 1);
    t.End();
});

test("worker: returns a value", t =>
{
    var value = Worker.Run(() => 42);
    t.Equal(value, 42);
    t.End();
});

test("worker: disabled flag reflects env", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", "1");
    var disabled = Worker.Disabled;
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", null);
    t.Ok(disabled);
    t.End();
});

test("worker: disabled mode runs inline", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", "1");
    var ran = false;
    Worker.Run(() => { ran = true; });
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", null);
    t.Ok(ran);
    t.End();
});

test("worker: disabled mode returns value", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", "1");
    var value = Worker.Run(() => "yes");
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", null);
    t.Equal(value, "yes");
    t.End();
});

test("worker: action runs on a worker thread by default", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", null);
    var flag = false;
    Worker.Run(() => { flag = true; });
    t.Ok(flag);
    t.End();
});

test("worker: func runs on a worker thread and returns", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_NO_WORKER", null);
    var value = Worker.Run(() => 7);
    t.Equal(value, 7);
    t.End();
});