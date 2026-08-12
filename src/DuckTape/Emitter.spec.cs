using DuckTape;
using static DuckTape.Tests;

Test("emitter: On/Emit basic", t =>
{
    var calls = new List<int>();
    var e = new Emitter();
    e.On("foo", _ => calls.Add(1));
    e.Emit("foo");
    t.Equal(calls.Count, 1);
    t.End();
});

Test("emitter: Emit forwards data payload", t =>
{
    string? received = null;
    var e = new Emitter();
    e.On("d", d => received = d as string);
    e.Emit("d", "payload");
    t.Equal(received, "payload");
    t.End();
});

Test("emitter: Off removes listener", t =>
{
    var calls = new List<int>();
    var e = new Emitter();
    Action<object?> fn = _ => calls.Add(1);
    e.On("x", fn);
    e.Off("x", fn);
    e.Emit("x");
    t.Equal(calls.Count, 0);
    t.End();
});

Test("emitter: multiple listeners on same event", t =>
{
    var calls = new List<int>();
    var e = new Emitter();
    e.On("x", _ => calls.Add(1));
    e.On("x", _ => calls.Add(2));
    e.Emit("x");
    t.DeepEqual(calls, new List<int> { 1, 2 });
    t.End();
});

Test("emitter: events without listeners do nothing", t =>
{
    var e = new Emitter();
    e.Emit("nothing");
    t.Ok(true);
    t.End();
});

Test("emitter: Off for unknown event is a noop", t =>
{
    var e = new Emitter();
    e.Off("nope", _ => { });
    t.Ok(true);
    t.End();
});

Test("emitter: listeners are distinct per event", t =>
{
    var a = new List<int>();
    var e = new Emitter();
    e.On("a", _ => a.Add(1));
    e.Emit("b");
    t.Equal(a.Count, 0);
    t.End();
});