using DuckTape;
using static DuckTape.Test;

Run("diff: equal values produce empty string", t =>
{
    t.Equal(Diff.MakeDiff(1, 1), string.Empty);
    t.End();
});

Run("diff: equal nulls produce empty string", t =>
{
    t.Equal(Diff.MakeDiff(null, null), string.Empty);
    t.End();
});

Run("diff: null versus value renders null", t =>
{
    t.Ok(Diff.MakeDiff(null, 1).Contains("null"));
    t.End();
});

Run("diff: unequal values produce diff block", t =>
{
    t.Ok(Diff.MakeDiff(1, 2).Contains("diff: |-"));
    t.End();
});

Run("diff: diff contains minus line for expected", t =>
{
    t.Match(Diff.MakeDiff("hello", "world"), @"-.*hello");
    t.End();
});

Run("diff: diff contains plus line for result", t =>
{
    t.Match(Diff.MakeDiff("hello", "world"), @"\+.*world");
    t.End();
});

Run("diff: equal dicts are diff free", t =>
{
    var a = new Dictionary<string, int> { ["x"] = 1 };
    var b = new Dictionary<string, int> { ["x"] = 1 };
    t.Equal(Diff.MakeDiff(a, b), string.Empty);
    t.End();
});