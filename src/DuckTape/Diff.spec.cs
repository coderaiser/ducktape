using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("diff: equal values produce empty string", t =>
{
    t.Equal(Diff.MakeDiff(1, 1), string.Empty);
    t.End();
});

test("diff: equal nulls produce empty string", t =>
{
    t.Equal(Diff.MakeDiff(null, null), string.Empty);
    t.End();
});

test("diff: null versus value renders null", t =>
{
    t.Ok(Diff.MakeDiff(null, 1).Contains("null"));
    t.End();
});

test("diff: unequal values produce diff block", t =>
{
    t.Ok(Diff.MakeDiff(1, 2).Contains("diff: |-"));
    t.End();
});

test("diff: diff contains minus line for expected", t =>
{
    t.Match(Diff.MakeDiff("hello", "world"), @"-.*hello");
    t.End();
});

test("diff: diff contains plus line for result", t =>
{
    t.Match(Diff.MakeDiff("hello", "world"), @"\+.*world");
    t.End();
});

test("diff: equal dicts are diff free", t =>
{
    var a = new Dictionary<string, int> { ["x"] = 1 };
    var b = new Dictionary<string, int> { ["x"] = 1 };
    t.Equal(Diff.MakeDiff(a, b), string.Empty);
    t.End();
});