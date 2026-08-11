using DuckTape.Formatter;

namespace DuckTape;
public record RunnerState(
    IFormatter Formatter,
    Func<int> Count,
    Action IncPassed,
    Action IncFailed
);

public class T
{
    readonly RunnerState _state;
    bool _ended;
    bool _afterEndEmitted;
    public int AssertionsCount { get; private set; }

    internal T(RunnerState state) => _state = state;

    void Run(string name, TestState ts)
    {
        if (_ended)
        {
            AssertAfterEnd();
            return;
        }
        AssertionsCount++;
        EmitResult(name, ts);
    }

    // A single, guarded failure for asserting (or calling End) more than once.
    void AssertAfterEnd()
    {
        if (_afterEndEmitted) return;
        _afterEndEmitted = true;
        AssertionsCount++;
        EmitResult("fail", Operators.Fail(new Exception("Cannot assert after End()")));
    }

    void EmitResult(string name, TestState ts)
    {
        if (ts.IsOk)
        {
            _state.IncPassed();
            _state.Formatter.Emit("test:success", new { count = _state.Count(), message = ts.Message });
        }
        else
        {
            _state.IncFailed();
            _state.Formatter.Emit("test:fail", new
            {
                count     = _state.Count(),
                message   = ts.Message,
                @operator = name,
                result    = ts.Result,
                expected  = ts.Expected,
                output    = ts.Output,
                at        = ts.At,
                error_stack = ts.Stack,
            });
        }
    }

    public void End()
    {
        if (_ended)
        {
            AssertAfterEnd();
            return;
        }
        _ended = true;
    }

    public void Equal<T1>(T1 result, T1 expected, string message = "should equal") =>
        Run("equal", Operators.Equal(result, expected, message));

    public void NotEqual<T1>(T1 result, T1 expected, string message = "should not equal") =>
        Run("not_equal", Operators.NotEqual(result, expected, message));

    public void Ok(object? result, string message = "should be truthy") =>
        Run("ok", Operators.Ok(result, message));

    public void NotOk(object? result, string message = "should be falsy") =>
        Run("not_ok", Operators.NotOk(result, message));

    public void DeepEqual<T1>(T1 result, T1 expected, string message = "should deep equal") =>
        Run("deep_equal", Operators.DeepEqual(result, expected, message));

    public void Match(string result, string pattern, string message = "should match") =>
        Run("match", Operators.Match(result, pattern, message));

    public void NotMatch(string result, string pattern, string message = "should not match") =>
        Run("not_match", Operators.NotMatch(result, pattern, message));

    public void Pass(string message = "(unnamed assert)") =>
        Run("pass", Operators.Pass(message));

    public void Fail(Exception error, string at = "") =>
        Run("fail", Operators.Fail(error, at));

    public void Comment(string message) =>
        _state.Formatter.Emit("comment", new { message });
}
