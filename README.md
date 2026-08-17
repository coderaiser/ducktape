# DuckTape

supertape-style test runner for C# — one assertion per test, `t.Equal(...)`, `t.End()`, functional entry via `CreateTest()`, no external test framework.

## Philosophy

- **One assertion per test** — each test makes exactly one `t.Equal`, `t.Ok`, `t.Match`, etc.
- **`t.End()`** — called once to close a test.
- **Functional entry** — `var test = CreateTest()` returns a function that accepts both sync and async callbacks.
- **No framework dependency** — DuckTape compiles and runs its own `.spec.cs` files via Roslyn at runtime.

## Quick Start

```bash
task test                       # run all spec files
task coverage                   # measure line coverage (requires 100%)
task lint                       # check dotnet formatting
task build                      # build the project
task publish                    # self-contained binaries for all platforms
```

### Run directly

```bash
dotnet run --project src/DuckTape -- 'src/**/*.spec.cs'
CI=1 dotnet run --project src/DuckTape -- 'src/**/*.spec.cs'
```

> Set `CI=1` (not `CI=true`) for TAP output and to disable the progress bar.

## Writing a Spec File

Each spec file is a self-contained C# file with top-level statements:

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

### Spec File Rules

1. One assertion per test (enforced by `DUCKTAPE_CHECK_ASSERTIONS_COUNT`).
2. Message format: `"scope: subject"` (enforced by `DUCKTAPE_CHECK_SCOPES`).
3. `t.End()` called exactly once.
4. No external test framework — DuckTape compiles spec files via Roslyn.
5. Each spec file starts with `using DuckTape;` and `using static DuckTape.Test;`.
6. Entry point: `var test = CreateTest();` at the top.
7. No `Skip` unless the feature is genuinely not yet implemented.

## API

### `CreateTest()`

Returns `Action<string, Func<T, Task>>` — a test registration function that handles both sync and async callbacks (mirrors supertape's `createTest()`).

```csharp
var test = CreateTest();

test("sync test", t => { t.Ok(true); t.End(); });
test("async test", async t => { await Task.Delay(0); t.Ok(true); t.End(); });
```

### `Only` / `Skip`

```csharp
Only("only: this runs", t => { t.Ok(true); t.End(); });
Skip("skip: this is skipped", t => { t.Ok(true); t.End(); });
```

### Operators (`t.*`)

| Operator | Description |
|---|---|
| `t.Equal(result, expected)` | Strict equality |
| `t.NotEqual(result, expected)` | Strict inequality |
| `t.DeepEqual(result, expected)` | Structural equality via JSON |
| `t.NotDeepEqual(result, expected)` | Structural inequality via JSON |
| `t.Ok(result)` | Truthy check |
| `t.NotOk(result)` | Falsy check |
| `t.Match(result, pattern)` | Regex match |
| `t.NotMatch(result, pattern)` | Regex non-match |
| `t.Pass()` | Always passes |
| `t.Fail(error)` | Always fails |
| `t.Comment(message)` | Emit a comment |
| `t.End()` | End the test |

## CLI

```
ducktape [options] <patterns...>
```

### Options

| Option | Description |
|---|---|
| `-h, --help` | Show help |
| `-v, --version` | Print version |
| `-f, --format <name>` | `tap` \| `fail` \| `short` \| `progress-bar` \| `json-lines` |
| `--no-worker` | Run on a single thread |
| `--no-check-duplicates` | Disable duplicate message check |
| `--no-check-assertions-count` | Disable one-assertion-per-test check |
| `--no-check-scopes` | Disable `scope: subject` format check |

## Formatters

| Format | Description |
|---|---|
| `tap` | TAP 13 — default in CI |
| `fail` | TAP showing only failures |
| `short` | Compact one-line-per-test summary |
| `json-lines` | One JSON object per line |
| `progress-bar` | Animated bar — default in terminals |

## Environment Variables

| Variable | Default | Controls |
|---|---|---|
| `DUCKTAPE_TIMEOUT` | `3000` | Per-test timeout (ms) |
| `DUCKTAPE_CHECK_DUPLICATES` | `1` | Duplicate message check |
| `DUCKTAPE_CHECK_ASSERTIONS_COUNT` | `1` | Exactly-one-assertion check |
| `DUCKTAPE_CHECK_SCOPES` | `0` | `scope: subject` format check |
| `DUCKTAPE_CHECK_SKIPPED` | `0` | Exit `Skipped` when skipped > 0 |
| `DUCKTAPE_NO_WORKER` | unset | `1` = single-thread |
| `DUCKTAPE_PROGRESS_BAR` | unset | `1` force on / `0` force off |
| `DUCKTAPE_PROGRESS_BAR_MIN` | `100` | Min tests to show bar |
| `DUCKTAPE_PROGRESS_BAR_STACK` | `1` | Show stack in progress-bar failures |

> `CI=1` triggers the `tap` format default and disables the progress bar.

## Exit Codes

| Code | Meaning |
|---|---|
| `0` | All tests passed |
| `1` | Test failure(s) |
| `2` | WasStop |
| `3` | Unhandled exception |
| `4` | Invalid option |
| `5` | Skipped |

## Coverage

Line coverage must be 100%. Measure with:

```bash
task coverage
```

Uses [coverlet](https://github.com/coverlet-coverage/coverlet) with `--threshold 100 --threshold-type line`.
