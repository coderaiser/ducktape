using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

// Test all operators with various inputs for branch coverage
test("ok: null is falsy", t =>
{
    var r = t.Ok(null!);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("ok: true is truthy", t =>
{
    var r = t.Ok(true);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("ok: 0 is falsy", t =>
{
    var r = t.Ok(0);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("ok: empty string is falsy", t =>
{
    var r = t.Ok("");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("ok: non-empty string is truthy", t =>
{
    var r = t.Ok("hello");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("not_ok: null is falsy", t =>
{
    var r = t.NotOk(null!);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("not_ok: false is falsy", t =>
{
    var r = t.NotOk(false);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("not_ok: 0 is falsy", t =>
{
    var r = t.NotOk(0);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("not_ok: empty string is falsy", t =>
{
    var r = t.NotOk("");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("not_ok: non-empty string is truthy", t =>
{
    var r = t.NotOk("hello");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("equal: equal values", t =>
{
    var r = t.Equal(42, 42);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("equal: different ints", t =>
{
    var r = t.Equal(42, 100);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("equal: equal nulls", t =>
{
    var r = t.Equal(null!, null!);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("equal: null vs value", t =>
{
    var r = t.Equal(1, null!);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("not_equal: not equal values", t =>
{
    var r = t.NotEqual(1, 2);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("not_equal: equal values", t =>
{
    var r = t.NotEqual(1, 1);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("deep_equal: equal arrays", t =>
{
    var r = t.DeepEqual(new[] { 1, 2 }, new[] { 1, 2 });
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("deep_equal: different arrays", t =>
{
    var r = t.DeepEqual(new[] { 1, 2 }, new[] { 1, 3 });
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("deep_equal: equal dictionaries", t =>
{
    var r = t.DeepEqual(new Dictionary<string, int> { ["x"] = 1 }, new Dictionary<string, int> { ["x"] = 1 });
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("deep_equal: different dictionaries", t =>
{
    var r = t.DeepEqual(new Dictionary<string, int> { ["x"] = 1 }, new Dictionary<string, int> { ["x"] = 2 });
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("match: matches pattern", t =>
{
    var r = t.Match("hello world", @"world");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("match: no match", t =>
{
    var r = t.Match("hello", @"world");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("not_match: no match becomes match", t =>
{
    var r = t.NotMatch("hello", @"world");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("not_match: match becomes no match", t =>
{
    var r = t.NotMatch("hello", @"hello");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("pass: always passes", t =>
{
    var r = t.Pass();
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("fail: records exception", t =>
{
    var r = t.Fail(new Exception("test error"));
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});
