using DuckTape.Cli;
using static DuckTape.Tests;

Test("args: default format outside CI is progress-bar", t =>
{
    Environment.SetEnvironmentVariable("CI", null);
    var args = ArgParser.Parse(Array.Empty<string>());
    t.Equal(args.Format, "progress-bar");
    t.End();
});

Test("args: default format in CI is tap", t =>
{
    Environment.SetEnvironmentVariable("CI", "1");
    var args = ArgParser.Parse(Array.Empty<string>());
    Environment.SetEnvironmentVariable("CI", null);
    t.Equal(args.Format, "tap");
    t.End();
});

Test("args: -f flag overrides default", t =>
{
    var args = ArgParser.Parse(new[] { "-f", "fail" });
    t.Equal(args.Format, "fail");
    t.End();
});

Test("args: --format flag overrides default", t =>
{
    var args = ArgParser.Parse(new[] { "--format", "short" });
    t.Equal(args.Format, "short");
    t.End();
});

Test("args: every valid format is accepted", t =>
{
    var ok = true;
    foreach (var f in new[] { "tap", "fail", "short", "progress-bar", "json-lines" })
        if (ArgParser.Parse(new[] { "-f", f }).Format != f) ok = false;
    t.Ok(ok);
    t.End();
});

Test("args: --no-worker sets flag", t =>
{
    var args = ArgParser.Parse(new[] { "--no-worker" });
    t.Ok(args.NoWorker);
    t.End();
});

Test("args: --no-check-duplicates sets flag", t =>
{
    var args = ArgParser.Parse(new[] { "--no-check-duplicates" });
    t.Ok(args.NoCheckDuplicates);
    t.End();
});

Test("args: --no-check-assertions-count sets flag", t =>
{
    var args = ArgParser.Parse(new[] { "--no-check-assertions-count" });
    t.Ok(args.NoCheckAssertionsCount);
    t.End();
});

Test("args: -h sets help", t =>
{
    var args = ArgParser.Parse(new[] { "-h" });
    t.Ok(args.Help);
    t.End();
});

Test("args: --help sets help", t =>
{
    var args = ArgParser.Parse(new[] { "--help" });
    t.Ok(args.Help);
    t.End();
});

Test("args: -v sets version", t =>
{
    var args = ArgParser.Parse(new[] { "-v" });
    t.Ok(args.Version);
    t.End();
});

Test("args: --version sets version", t =>
{
    var args = ArgParser.Parse(new[] { "--version" });
    t.Ok(args.Version);
    t.End();
});

Test("args: positional values become patterns", t =>
{
    var args = ArgParser.Parse(new[] { "a", "b" });
    t.DeepEqual(args.Patterns, new[] { "a", "b" });
    t.End();
});

Test("args: no patterns by default", t =>
{
    var args = ArgParser.Parse(Array.Empty<string>());
    t.Equal(args.Patterns.Length, 0);
    t.End();
});

Test("args: unknown format throws", t =>
{
    var threw = false;
    try { ArgParser.Parse(new[] { "-f", "junit" }); }
    catch (InvalidFormatException ex) { threw = ex.Message.Contains("junit"); }
    t.Ok(threw);
    t.End();
});

Test("args: missing format value throws", t =>
{
    var threw = false;
    try { ArgParser.Parse(new[] { "-f" }); }
    catch (InvalidFormatException) { threw = true; }
    t.Ok(threw);
    t.End();
});