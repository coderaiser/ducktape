using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

// Test all operators with various inputs for branch coverage
test("operators: ok null is falsy", t =>
{
    var r = Operators.Ok(null!);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: ok true is truthy", t =>
{
    var r = Operators.Ok(true);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: ok 0 is falsy", t =>
{
    var r = Operators.Ok(0);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: ok empty string is falsy", t =>
{
    var r = Operators.Ok("");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: ok non-empty string is truthy", t =>
{
    var r = Operators.Ok("hello");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_ok null is ok", t =>
{
    var r = Operators.NotOk(null!);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_ok false is ok", t =>
{
    var r = Operators.NotOk(false);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_ok 0 is ok", t =>
{
    var r = Operators.NotOk(0);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_ok empty string is ok", t =>
{
    var r = Operators.NotOk("");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_ok non-empty string fails", t =>
{
    var r = Operators.NotOk("hello");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: ok non-zero int is truthy", t =>
{
    var r = Operators.Ok(3);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_ok 42 fails", t =>
{
    var r = Operators.NotOk(42);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: equal same values pass", t =>
{
    var r = Operators.Equal(42, 42);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: equal different ints fail", t =>
{
    var r = Operators.Equal(42, 100);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: equal nulls pass", t =>
{
    var r = Operators.Equal<object?>(null!, null!);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: equal object vs null fails", t =>
{
    var r = Operators.Equal<object?>(1, null!);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_equal different pass", t =>
{
    var r = Operators.NotEqual(1, 2);
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_equal same fails", t =>
{
    var r = Operators.NotEqual(1, 1);
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: deep_equal equal arrays", t =>
{
    var r = Operators.DeepEqual(new[] { 1, 2 }, new[] { 1, 2 });
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: deep_equal different arrays", t =>
{
    var r = Operators.DeepEqual(new[] { 1, 2 }, new[] { 1, 3 });
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: deep_equal equal dicts", t =>
{
    var r = Operators.DeepEqual(new Dictionary<string, int> { ["x"] = 1 }, new Dictionary<string, int> { ["x"] = 1 });
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: deep_equal different dicts", t =>
{
    var r = Operators.DeepEqual(new Dictionary<string, int> { ["x"] = 1 }, new Dictionary<string, int> { ["x"] = 2 });
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: match matches", t =>
{
    var r = Operators.Match("hello world", @"world");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: match no match", t =>
{
    var r = Operators.Match("hello", @"world");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_match no match", t =>
{
    var r = Operators.NotMatch("hello", @"world");
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: not_match match", t =>
{
    var r = Operators.NotMatch("hello", @"hello");
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});

test("operators: pass always passes", t =>
{
    var r = Operators.Pass();
    t.Equal(r.IsOk, true);
    t.End();
    return Task.CompletedTask;
});

test("operators: fail records exception", t =>
{
    var r = Operators.Fail(new Exception("test error"));
    t.Equal(r.IsOk, false);
    t.End();
    return Task.CompletedTask;
});
