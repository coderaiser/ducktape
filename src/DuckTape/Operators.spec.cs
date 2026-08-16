using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("operators: Equal passes when values match", t =>
{
    t.Ok(Operators.Equal(1, 1).IsOk);
    t.End();
});

test("operators: Equal fails when values differ", t =>
{
    t.NotOk(Operators.Equal(1, 2).IsOk);
    t.End();
});

test("operators: Equal produces diff on failure", t =>
{
    t.Ok(Operators.Equal(1, 2).Output.Length > 0);
    t.End();
});

test("operators: Equal uses custom message", t =>
{
    t.Equal(Operators.Equal(1, 1, "should match").Message, "should match");
    t.End();
});

test("operators: Ok passes for truthy", t =>
{
    t.Ok(Operators.Ok("hello").IsOk);
    t.End();
});

test("operators: Ok fails for falsy", t =>
{
    t.NotOk(Operators.Ok("").IsOk);
    t.End();
});

test("operators: Ok fails for null", t =>
{
    t.NotOk(Operators.Ok(null).IsOk);
    t.End();
});

test("operators: Ok fails for zero", t =>
{
    t.NotOk(Operators.Ok(0).IsOk);
    t.End();
});

test("operators: NotOk passes for falsy", t =>
{
    t.Ok(Operators.NotOk(0).IsOk);
    t.End();
});

test("operators: NotOk fails for truthy", t =>
{
    t.NotOk(Operators.NotOk("x").IsOk);
    t.End();
});

test("operators: NotEqual passes when values differ", t =>
{
    t.Ok(Operators.NotEqual(1, 2).IsOk);
    t.End();
});

test("operators: NotEqual fails when values match", t =>
{
    t.NotOk(Operators.NotEqual(1, 1).IsOk);
    t.End();
});

test("operators: DeepEqual passes for equal dicts", t =>
{
    var a = new Dictionary<string, int> { ["x"] = 1 };
    var b = new Dictionary<string, int> { ["x"] = 1 };
    t.Ok(Operators.DeepEqual(a, b).IsOk);
    t.End();
});

test("operators: DeepEqual fails for differing dicts", t =>
{
    var a = new Dictionary<string, int> { ["x"] = 1 };
    var b = new Dictionary<string, int> { ["x"] = 2 };
    t.NotOk(Operators.DeepEqual(a, b).IsOk);
    t.End();
});

test("operators: Match passes when regex matches", t =>
{
    t.Ok(Operators.Match("hello world", @"world").IsOk);
    t.End();
});

test("operators: Match fails when no match", t =>
{
    t.NotOk(Operators.Match("hello", @"world").IsOk);
    t.End();
});

test("operators: NotMatch passes when regex does not match", t =>
{
    t.Ok(Operators.NotMatch("hello", @"world").IsOk);
    t.End();
});

test("operators: NotMatch fails when regex matches", t =>
{
    t.NotOk(Operators.NotMatch("hello", @"llo").IsOk);
    t.End();
});

test("operators: Pass is ok", t =>
{
    t.Ok(Operators.Pass().IsOk);
    t.End();
});

test("operators: Pass uses unnamed default message", t =>
{
    t.Equal(Operators.Pass().Message, "(unnamed assert)");
    t.End();
});

test("operators: Fail carries exception message", t =>
{
    t.Equal(Operators.Fail(new InvalidOperationException("boom")).Message, "boom");
    t.End();
});

test("operators: Fail carries location", t =>
{
    t.Equal(Operators.Fail(new Exception("x"), "file.cs:2").At, "file.cs:2");
    t.End();
});

test("operators: Fail carries stack trace", t =>
{
    Exception ex;
    try { throw new Exception("x"); }
    catch (Exception e) { ex = e; }
    t.Ok(Operators.Fail(ex).Stack.Length > 0);
    t.End();
});