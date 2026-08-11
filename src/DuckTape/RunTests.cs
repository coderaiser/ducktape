using DuckTape.Formatter;

namespace DuckTape;

public record RunResult(int Count, int Passed, int Failed, int Skipped);

public record TestDefinition(
    string Message,
    Func<T, Task> Fn,
    bool Skip = false,
    bool Only = false,
    string At = ""
);

public static class Runner
{
    public static async Task<RunResult> RunTests(
        List<TestDefinition> tests, IFormatter formatter)
    {
        var active = tests.Any(t => t.Only)
            ? tests.Where(t => t.Only).ToList()
            : tests.Where(t => !t.Skip).ToList();

        var skipped = tests.Count - active.Count;
        return await RunList(active, skipped, formatter);
    }

    static async Task<RunResult> RunList(
        List<TestDefinition> active, int skipped, IFormatter formatter)
    {
        int count = 0, passed = 0, failed = 0;
        var validator = new Validator(active);

        formatter.Emit("start", new { total = active.Count });

        foreach (var def in active)
        {
            formatter.Emit("test", new { test = def.Message });

            var state = new RunnerState(
                Formatter: formatter,
                Count: () => count,
                IncPassed: () => passed++,
                IncFailed: () => failed++
            );
            var t = new T(state);

            var timeoutMs = int.TryParse(
                Environment.GetEnvironmentVariable("DUCKTAPE_TIMEOUT"), out var ms) ? ms : 3000;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                await def.Fn(t).WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                t.Fail(new Exception($"Test timed out after {timeoutMs}ms"));
                t.End();
            }
            catch (Exception ex)
            {
                t.Fail(ex);
                t.End();
            }

            count++;

            var v = validator.Validate(def.Message, t.AssertionsCount);
            if (v.Message is not null)
            {
                var t2 = new T(state);
                t2.Fail(new Exception(v.Message), v.At ?? "");
                t2.End();
                count++;
            }

            formatter.Emit("test:end", new { count, total = active.Count, test = def.Message, failed });
        }

        formatter.Emit("end", new { count, passed, failed, skipped });
        return new(count, passed, failed, skipped);
    }
}
