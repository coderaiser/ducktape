# DuckTape — Bootstrap Plan v9

**Philosophy:** one assertion per test, `t.Equal(...)`, `t.End()`, functional entry via `CreateTest()`, no framework dependency.

NuGet package name: `DuckTape`  
Target: `net10.0`  
Reference: `git clone --depth=1 https://github.com/coderaiser/ducktape`

---

## Critical Fix — TestLoader performance

**Problem:** `References()` rebuilt from scratch on every `Load()` call — ~200–300 DLL reads per
spec file, ~6,300 redundant disk reads total across 21 spec files → suite takes 5+ minutes.

**Fix:** cache in a static `Lazy<>` — built once, reused for all files. Also set
`OptimizationLevel.Release`.

```csharp
// Before (slow):
static List<MetadataReference> References() { /* walks TPA every call */ }

// After (fast — built once):
static readonly Lazy<List<MetadataReference>> _refs = new(BuildReferences);

static List<MetadataReference> BuildReferences()
{
    var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "";
    var refs = tpa
        .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
        .ToList();
    refs.Add(MetadataReference.CreateFromFile(typeof(Test).Assembly.Location));
    return refs;
}
```

```csharp
// Also update CSharpCompilationOptions:
new CSharpCompilationOptions(
    OutputKind.ConsoleApplication,
    optimizationLevel: OptimizationLevel.Release,
    xmlReferenceResolver: null)
```

**Result:** full 21-spec suite completes in under 30 seconds. ✅ Already landed in repo.

---

## API Fix — `CreateTest()` must handle both sync and async

**Problem:** `CreateTestAsync()` does not exist in supertape. In supertape `createTest()` returns
one function that handles both sync and async callbacks — there is no separate async variant.

**Current repo state (wrong):**
```csharp
public static Action<string, Action<T>> CreateTest() => ...       // sync only
public static Action<string, Func<T, Task>> CreateTestAsync() => ... // async only — does not exist in supertape
```

**Fix:** `CreateTest()` returns a delegate that accepts `Func<T, Task>` — covers both sync and
async, since `Action<T>` can be wrapped:

```csharp
public static Action<string, Func<T, Task>> CreateTest() =>
    (message, fn) => Add(message, fn, false, false);
```

Sync callers wrap automatically via a helper overload or implicit lambda conversion. Drop
`CreateTestAsync` entirely.

**Usage (after fix):**
```csharp
using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("parser: equal works", t =>
{
    t.Equal(42, 42);
    t.End();
});

test("parser: async works", async t =>
{
    await Task.Delay(0);
    t.Ok(true);
    t.End();
});
```

---

## Supertape Alignment

| supertape | DuckTape | Status |
|---|---|---|
| `createTest()` | `CreateTest()` | ✅ exists — needs async fix above |
| `createTestAsync()` | `CreateTestAsync()` | ❌ does not exist in supertape — remove |
| `test.only(...)` | `Only(...)` | ✅ works — tested in `RunTests.spec.cs` |
| `test.skip(...)` | `Skip(...)` | ✅ works — tested in `RunTests.spec.cs` |
| `t.equal` | `t.Equal` | ✅ |
| `t.notEqual` | `t.NotEqual` | ✅ |
| `t.ok` | `t.Ok` | ✅ |
| `t.notOk` | `t.NotOk` | ✅ |
| `t.deepEqual` | `t.DeepEqual` | ✅ |
| `t.notDeepEqual` | `t.NotDeepEqual` | ❌ missing |
| `t.match` | `t.Match` | ✅ |
| `t.notMatch` | `t.NotMatch` | ✅ |
| `t.pass` | `t.Pass` | ✅ |
| `t.fail` | `t.Fail` | ✅ |
| `t.comment` | `t.Comment` | ✅ |
| `t.end()` | `t.End()` | ✅ |
| formatter: `tap` | `TapFormatter` | ✅ fully implemented and tested |
| formatter: `fail` | `FailFormatter` | ✅ fully implemented and tested |
| formatter: `short` | `ShortFormatter` | ✅ fully implemented and tested |
| formatter: `json-lines` | `JsonLinesFormatter` | ✅ fully implemented and tested |
| formatter: `progress-bar` | `ProgressBarFormatter` | ✅ fully implemented and tested |
| duplicate check | `DUCKTAPE_CHECK_DUPLICATES` | ✅ |
| assertion count check | `DUCKTAPE_CHECK_ASSERTIONS_COUNT` | ✅ |
| scope check | ❌ missing (`DUCKTAPE_CHECK_SCOPES`) | not yet implemented |
| timeout | `DUCKTAPE_TIMEOUT` | ✅ |
| `--no-check-scopes` | ❌ missing | not yet implemented |

**Missing vs supertape:**
- `t.NotDeepEqual` — operator + spec
- `DUCKTAPE_CHECK_SCOPES` / `--no-check-scopes` — validator check that messages follow `scope: subject` format

---

## Usage

```csharp
using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("parser: equal works", t =>
{
    t.Equal(42, 42);
    t.End();
});
```

---

## Project Structure

```
ducktape/
├── DuckTape.sln
├── Taskfile.yml
├── NuGet.Config
├── README.md
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── release.yml
├── scripts/
│   └── publish.sh
└── src/
    └── DuckTape/
        ├── DuckTape.csproj
        ├── Program.cs
        ├── Test.cs
        ├── T.cs
        ├── T.spec.cs
        ├── Operators.cs
        ├── Operators.spec.cs
        ├── RunTests.cs
        ├── RunTests.spec.cs
        ├── Validator.cs
        ├── Validator.spec.cs
        ├── Emitter.cs
        ├── Emitter.spec.cs
        ├── Diff.cs
        ├── Diff.spec.cs
        ├── Format.cs
        ├── Format.spec.cs
        ├── ExitCodes.cs
        ├── ExitCodes.spec.cs
        ├── Entry.spec.cs
        ├── Worker.cs
        ├── Worker.spec.cs
        ├── Glob.cs
        ├── Glob.spec.cs
        ├── TestLoader.cs
        ├── TestLoader.spec.cs
        ├── Formatter/
        │   ├── IFormatter.cs
        │   ├── FormatterBase.cs
        │   ├── Harness.cs
        │   ├── Harness.spec.cs
        │   ├── FormatterResolver.cs
        │   ├── FormatterResolver.spec.cs
        │   ├── TapFormatter.cs
        │   ├── Tap.spec.cs
        │   ├── FailFormatter.cs
        │   ├── Fail.spec.cs
        │   ├── ShortFormatter.cs
        │   ├── Short.spec.cs
        │   ├── JsonLinesFormatter.cs
        │   ├── JsonLines.spec.cs
        │   ├── ProgressBarFormatter.cs
        │   └── ProgressBar.spec.cs
        └── Cli/
            ├── Args.cs
            ├── Args.spec.cs
            ├── Command.cs
            ├── Cli.spec.cs
            └── Help.cs
```

---

## Phase 0 — Scaffold

### `DuckTape.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <PackageId>DuckTape</PackageId>
    <Version>0.1.0</Version>
    <Description>supertape-style test runner for C#</Description>
    <AssemblyName>ducktape</AssemblyName>
    <RootNamespace>DuckTape</RootNamespace>
    <InvariantGlobalization>true</InvariantGlobalization>
    <NuGetAudit>false</NuGetAudit>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="**/*.spec.cs" />
  </ItemGroup>
</Project>
```

### `NuGet.Config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="./nuget-local" />
  </packageSources>
</configuration>
```

---

## Phase 1 — Taskfile

```yaml
version: '3'

vars:
  PROJECT: src/DuckTape
  SPECS: "'src/**/*.spec.cs'"
  BINARY_BASE: https://github.com/coderaiser/binaries/releases/download/dotnet
  NUGET_LOCAL: ./nuget-local
  TOOLS_DIR: ./tools

tasks:
  bootstrap:
    desc: Download SDK and NuGet packages from coderaiser/binaries
    cmds:
      - mkdir -p {{.TOOLS_DIR}}/dotnet {{.NUGET_LOCAL}}
      - curl -sL {{.BINARY_BASE}}/dotnet-sdk-10.0-linux-x64.tar.gz
          | tar xz -C {{.TOOLS_DIR}}/dotnet
      - curl -sL {{.BINARY_BASE}}/Microsoft.CodeAnalysis.CSharp.4.14.0.nupkg
          -o {{.NUGET_LOCAL}}/microsoft.codeanalysis.csharp.4.14.0.nupkg
      - curl -sL {{.BINARY_BASE}}/Microsoft.CodeAnalysis.Common.4.14.0.nupkg
          -o {{.NUGET_LOCAL}}/microsoft.codeanalysis.common.4.14.0.nupkg
      - curl -sL {{.BINARY_BASE}}/Microsoft.CodeAnalysis.Analyzers.3.11.0.nupkg
          -o {{.NUGET_LOCAL}}/microsoft.codeanalysis.analyzers.3.11.0.nupkg
    env:
      DOTNET_CLI_TELEMETRY_OPTOUT: "1"

  build:
    desc: Build the project
    cmds:
      - dotnet build {{.PROJECT}}
    env:
      DOTNET_CLI_TELEMETRY_OPTOUT: "1"

  lint:
    desc: Check formatting — no changes written
    cmds:
      - dotnet format {{.PROJECT}} --verify-no-changes --severity warn
    env:
      DOTNET_CLI_TELEMETRY_OPTOUT: "1"

  test:
    desc: Run all spec files
    cmds:
      - dotnet run --project {{.PROJECT}} -- {{.SPECS}}
    env:
      DOTNET_CLI_TELEMETRY_OPTOUT: "1"
      CI: "1"

  coverage:
    desc: Measure line coverage with coverlet; fail below 100%
    cmds:
      - dotnet build {{.PROJECT}}
      - coverlet {{.PROJECT}}/bin/Debug/net10.0/ducktape.dll
          --target {{.PROJECT}}/bin/Debug/net10.0/ducktape
          --targetargs {{.SPECS}}
          --format cobertura
          --output coverage.xml
          --include-test-assembly
          --threshold 100
          --threshold-type line
    env:
      DOTNET_CLI_TELEMETRY_OPTOUT: "1"
      CI: "1"

  publish:
    desc: Build self-contained binaries for all platforms into ./publish
    cmds:
      - for: [linux-x64, linux-arm64, win-x64, win-arm64, osx-x64, osx-arm64]
        cmd: >
          dotnet publish {{.PROJECT}}
          -c Release
          -r {{.ITEM}}
          --self-contained true
          -o publish/{{.ITEM}}
    env:
      DOTNET_CLI_TELEMETRY_OPTOUT: "1"
```

---

## Phase 2 — Core Modules

### `Test.cs` — fix `CreateTest()`, drop `CreateTestAsync()`

```csharp
namespace DuckTape;

public static class Test
{
    static readonly List<TestDefinition> _tests = new();

    /// <summary>
    /// Returns a local test function. Handles both sync and async callbacks.
    /// Usage: var test = CreateTest();
    /// Mirrors supertape's createTest() exactly — no separate async variant.
    /// </summary>
    public static Action<string, Func<T, Task>> CreateTest() =>
        (message, fn) => Add(message, fn, false, false);

    public static void Only(string message, Action<T> fn) =>
        Add(message, t => { fn(t); return Task.CompletedTask; }, false, true);

    public static void Skip(string message, Action<T> fn) =>
        Add(message, t => { fn(t); return Task.CompletedTask; }, true, false);

    static void Add(string message, Func<T, Task> fn, bool skip, bool only) =>
        _tests.Add(new TestDefinition(message, fn, skip, only, CallerAt()));

    internal static List<TestDefinition> All => _tests;

    static string CallerAt(
        [System.Runtime.CompilerServices.CallerFilePath] string file = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int line = 0) =>
        $"{file}:{line}";
}
```

Spec (`Entry.spec.cs`):

```csharp
using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("entry: sync callback runs", t =>
{
    t.Ok(true);
    t.End();
});

test("entry: async callback runs", async t =>
{
    await Task.CompletedTask;
    t.Ok(true);
    t.End();
});

test("entry: createTest returns independent fn", t =>
{
    var test2 = CreateTest();
    t.Ok(test2 is not null);
    t.End();
});

test("entry: skip registers a skipped test", t =>
{
    Skip("entry: skipped registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});

test("entry: only registers an only test", t =>
{
    Only("entry: only registration", t2 => { t2.Ok(true); t2.End(); });
    t.Ok(true);
    t.End();
});
```

### `Operators.cs` — add `NotDeepEqual`

```csharp
public static TestState NotDeepEqual<T>(T result, T expected, string message = "should not deep equal")
{
    var ra = System.Text.Json.JsonSerializer.Serialize(result);
    var rb = System.Text.Json.JsonSerializer.Serialize(expected);
    var isOk = ra != rb;
    return new(isOk, message, result, expected, isOk ? "" : Diff.MakeDiff(expected, result));
}
```

Add to `T.cs`:

```csharp
public void NotDeepEqual<T1>(T1 result, T1 expected, string message = "should not deep equal") =>
    Run("not_deep_equal", Operators.NotDeepEqual(result, expected, message));
```

Spec additions (`Operators.spec.cs`):

```csharp
test("operators: NotDeepEqual passes for different collections", t =>
{
    t.Ok(Operators.NotDeepEqual(new[] { 1, 2 }, new[] { 1, 3 }).IsOk);
    t.End();
});

test("operators: NotDeepEqual fails for equal collections", t =>
{
    t.NotOk(Operators.NotDeepEqual(new[] { 1, 2 }, new[] { 1, 2 }).IsOk);
    t.End();
});
```

### `Validator.cs` — add scope check

```csharp
static readonly System.Text.RegularExpressions.Regex _scopeRe =
    new(@"^[\w\-/\d\s]+:.*", System.Text.RegularExpressions.RegexOptions.Compiled);

public ValidationResult Validate(string message, int assertionsCount)
{
    if (CheckDuplicates()) { /* existing */ }

    if (CheckScopes())
    {
        if (!_scopeRe.IsMatch(message))
            return new($"Scope required: 'scope: subject', got: '{message}'", null);
    }

    if (CheckAssertionsCount()) { /* existing */ }

    return new(null, null);
}

static bool CheckScopes() =>
    Environment.GetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES") == "1";
```

### `Cli/Args.cs` — add `--no-check-scopes`

```csharp
public record Args(
    string Format,
    bool NoWorker,
    bool NoCheckDuplicates,
    bool NoCheckAssertionsCount,
    bool NoCheckScopes,          // new
    bool Help,
    bool Version,
    string[] Patterns
);

// in Parse():
case "--no-check-scopes": noCheckScopes = true; break;
```

In `Command.cs`:

```csharp
if (args.NoCheckScopes)
    Environment.SetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES", "0");
```

---

## Phase 3 — Formatters (all implemented ✅)

All five formatters are complete and tested:

| Formatter | File | Tested |
|---|---|---|
| `tap` | `TapFormatter.cs` | `Tap.spec.cs` — 11 tests |
| `fail` | `FailFormatter.cs` | `Fail.spec.cs` — 6 tests |
| `short` | `ShortFormatter.cs` | `Short.spec.cs` — 8 tests |
| `json-lines` | `JsonLinesFormatter.cs` | `JsonLines.spec.cs` — 7 tests |
| `progress-bar` | `ProgressBarFormatter.cs` | `ProgressBar.spec.cs` — 12 tests |

`Only` and `Skip` are tested in `RunTests.spec.cs`:
- `"run_tests: skipped tests do not run"` — Skip ✅
- `"run_tests: only tests skip others"` — Only ✅

---

## Phase 4 — GitHub Actions

### `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Build
        run: dotnet build src/DuckTape
        env:
          DOTNET_CLI_TELEMETRY_OPTOUT: "1"

      - name: Lint
        run: dotnet format src/DuckTape --verify-no-changes --severity warn
        env:
          DOTNET_CLI_TELEMETRY_OPTOUT: "1"

      - name: Test
        run: dotnet run --project src/DuckTape -- 'src/**/*.spec.cs'
        env:
          DOTNET_CLI_TELEMETRY_OPTOUT: "1"
          CI: "1"
```

### `.github/workflows/release.yml`

Triggered by a version tag (`v*`). Builds self-contained binaries for all six targets, archives
them, and attaches them to the GitHub release.

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    name: Build ${{ matrix.rid }}
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        include:
          - rid: linux-x64
            os: ubuntu-latest
          - rid: linux-arm64
            os: ubuntu-latest
          - rid: win-x64
            os: windows-latest
          - rid: win-arm64
            os: windows-latest
          - rid: osx-x64
            os: macos-latest
          - rid: osx-arm64
            os: macos-latest

    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Publish
        run: >
          dotnet publish src/DuckTape
          -c Release
          -r ${{ matrix.rid }}
          --self-contained true
          -o publish/${{ matrix.rid }}
        env:
          DOTNET_CLI_TELEMETRY_OPTOUT: "1"

      - name: Archive (Unix)
        if: runner.os != 'Windows'
        run: |
          cd publish/${{ matrix.rid }}
          tar czf ../../ducktape-${{ matrix.rid }}.tar.gz ducktape
        shell: bash

      - name: Archive (Windows)
        if: runner.os == 'Windows'
        run: |
          cd publish/${{ matrix.rid }}
          Compress-Archive -Path ducktape.exe -DestinationPath ../../ducktape-${{ matrix.rid }}.zip
        shell: pwsh

      - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
          name: ducktape-${{ matrix.rid }}
          path: |
            ducktape-${{ matrix.rid }}.tar.gz
            ducktape-${{ matrix.rid }}.zip
          if-no-files-found: ignore

  release:
    needs: build
    runs-on: ubuntu-latest
    permissions:
      contents: write
    steps:
      - uses: actions/download-artifact@v4
        with:
          path: artifacts
          merge-multiple: true

      - name: Create release
        uses: softprops/action-gh-release@v2
        with:
          files: artifacts/*
```

---

## Phase 5 — README

See `README.md` below — written as a separate deliverable.

---

## Phase 6 — Self-test

```bash
task test
```

Expected: exit code 0, all tests pass in under 30 seconds.

---

## Environment Variables

| Variable | Default | Controls |
|---|---|---|
| `DUCKTAPE_TIMEOUT` | `3000` ms | per-test timeout |
| `DUCKTAPE_CHECK_DUPLICATES` | `1` | duplicate message check |
| `DUCKTAPE_CHECK_ASSERTIONS_COUNT` | `1` | exactly-one-assertion check |
| `DUCKTAPE_CHECK_SCOPES` | `0` | `scope: subject` format check |
| `DUCKTAPE_CHECK_SKIPPED` | `0` | exit `Skipped` when skipped > 0 |
| `DUCKTAPE_NO_WORKER` | unset | `1` = single-thread |
| `DUCKTAPE_PROGRESS_BAR` | unset | `1` force on / `0` force off |
| `DUCKTAPE_PROGRESS_BAR_MIN` | `100` | min tests to show bar |
| `DUCKTAPE_PROGRESS_BAR_STACK` | `1` | show stack in progress-bar failures |

> `CI=1` (not `CI=true`) triggers the `tap` format default and disables the progress bar.

---

## Commit Convention

```
feature: scope: message
test: scope: message
```

No `-` in commit messages. Examples:
- `feature: test: drop CreateTestAsync, make CreateTest handle async`
- `feature: operators: add NotDeepEqual`
- `feature: validator: add scope check`
- `test: operators: cover NotDeepEqual pass and fail`
- `test: entry: cover async callback via CreateTest`

---

## Spec File Rules

1. One assertion per test (`DUCKTAPE_CHECK_ASSERTIONS_COUNT`).
2. Message format: `"scope: subject"`.
3. `t.End()` called exactly once.
4. No external test framework — DuckTape compiles and runs its own spec files via Roslyn.
5. Each spec file starts with `using DuckTape;` and `using static DuckTape.Test;`.
6. Entry point: `var test = CreateTest();` at the top of each spec file.
7. No `Skip` unless the feature is genuinely not yet implemented.
