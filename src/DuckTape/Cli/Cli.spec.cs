using DuckTape;
using DuckTape.Cli;
using DuckTape.Formatter;
using static DuckTape.Tests;

CliDependencies NoopDeps(List<TestDefinition>? tests = null) => new(
    _ => { },
    () => tests ?? new List<TestDefinition>(),
    (_, s) => new TapFormatter(s));

Test("cli: help prints usage and exits ok", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var code = CliRunner.Run(new[] { "-h" }, sw, err);
    t.Ok(code == ExitCodes.Ok && sw.ToString().Contains("Usage"));
    t.End();
});

Test("cli: --help prints usage too", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var code = CliRunner.Run(new[] { "--help" }, sw, err);
    t.Ok(code == ExitCodes.Ok && sw.ToString().Contains("DUCKTAPE_TIMEOUT"));
    t.End();
});

Test("cli: version prints the version", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var code = CliRunner.Run(new[] { "--version" }, sw, err);
    t.Ok(code == ExitCodes.Ok && sw.ToString().Contains("0.1.0"));
    t.End();
});

Test("cli: no matches is invalid option", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var code = CliRunner.Run(new[] { "nope*.cs" }, sw, err);
    t.Ok(code == ExitCodes.InvalidOption && err.ToString().Contains("no test files matched"));
    t.End();
});

Test("cli: invalid format is invalid option", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var code = CliRunner.Run(new[] { "-f", "bogus" }, sw, err);
    t.Ok(code == ExitCodes.InvalidOption && err.ToString().Contains("unknown format"));
    t.End();
});

Test("cli: passing suite exits zero", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var dir = Directory.CreateTempSubdirectory("ducktape_cli_");
    File.WriteAllText(Path.Combine(dir.FullName, "x.spec.cs"), "// empty");
    var deps = NoopDeps(new List<TestDefinition>
    {
        new("cli: local pass", t2 => { t2.Ok(true); t2.End(); return Task.CompletedTask; }),
    });
    var code = CliRunner.Execute(new[] { Path.Combine(dir.FullName, "*.spec.cs") }, sw, err, deps);
    Directory.Delete(dir.FullName, true);
    t.Ok(code == ExitCodes.Ok && sw.ToString().Contains("TAP version 13"));
    t.End();
});

Test("cli: failing suite exits one", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var dir = Directory.CreateTempSubdirectory("ducktape_cli_");
    File.WriteAllText(Path.Combine(dir.FullName, "x.spec.cs"), "// empty");
    var deps = NoopDeps(new List<TestDefinition>
    {
        new("cli: local fail", t2 => { t2.Equal(1, 2); t2.End(); return Task.CompletedTask; }),
    });
    var code = CliRunner.Execute(new[] { Path.Combine(dir.FullName, "*.spec.cs") }, sw, err, deps);
    Directory.Delete(dir.FullName, true);
    t.Equal(code, ExitCodes.Fail);
    t.End();
});

Test("cli: loader errors are surfaced as failures", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var dir = Directory.CreateTempSubdirectory("ducktape_cli_");
    File.WriteAllText(Path.Combine(dir.FullName, "x.spec.cs"), "// empty");
    var deps = new CliDependencies(
        _ => throw new InvalidOperationException("boom"),
        () => new List<TestDefinition>(),
        (_, s) => new TapFormatter(s));
    var code = CliRunner.Execute(new[] { Path.Combine(dir.FullName, "*.spec.cs") }, sw, err, deps);
    Directory.Delete(dir.FullName, true);
    t.Ok(code == ExitCodes.Fail && err.ToString().Contains("boom"));
    t.End();
});

Test("cli: skipped tests honour check flag", t =>
{
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SKIPPED", "1");
    var sw = new StringWriter();
    var err = new StringWriter();
    var dir = Directory.CreateTempSubdirectory("ducktape_cli_");
    File.WriteAllText(Path.Combine(dir.FullName, "x.spec.cs"), "// empty");
    var deps = NoopDeps(new List<TestDefinition>
    {
        new("cli: local skip", t2 => { t2.Ok(true); t2.End(); return Task.CompletedTask; }, Skip: true),
    });
    var code = CliRunner.Execute(new[] { Path.Combine(dir.FullName, "*.spec.cs") }, sw, err, deps);
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SKIPPED", null);
    Directory.Delete(dir.FullName, true);
    t.Equal(code, ExitCodes.Skipped);
    t.End();
});

Test("cli: no_check_duplicates writes env before exit", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    var code = CliRunner.Execute(new[] { "--no-check-duplicates" }, sw, err, NoopDeps());
    var env = Environment.GetEnvironmentVariable("DUCKTAPE_CHECK_DUPLICATES");
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_DUPLICATES", null);
    t.Ok(code == ExitCodes.InvalidOption && env == "0");
    t.End();
});

Test("cli: no_check_assertions_count writes env", t =>
{
    var sw = new StringWriter();
    var err = new StringWriter();
    CliRunner.Execute(new[] { "--no-check-assertions-count" }, sw, err, NoopDeps());
    var env = Environment.GetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT");
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT", null);
    t.Equal(env, "0");
    t.End();
});

Test("cli: default deps resolve non bar formats to stdout", t =>
{
    var resolved = CliRunner.Default.Resolve("tap", new StringWriter());
    t.Ok(resolved is TapFormatter);
    t.End();
});

Test("cli: plain run routes to console", t =>
{
    var code = CliRunner.Run(new[] { "-h" });
    t.Equal(code, ExitCodes.Ok);
    t.End();
});