namespace DuckTape;

/// <summary>
/// Runs a delegate on a worker thread, with a deterministic single-threaded
/// fallback controlled by the DUCKTAPE_NO_WORKER environment variable.
/// </summary>
public static class Worker
{
    public static bool Disabled =>
        Environment.GetEnvironmentVariable("DUCKTAPE_NO_WORKER") == "1";

    public static void Run(Action action)
    {
        if (Disabled) { action(); return; }
        Task.Run(action).GetAwaiter().GetResult();
    }

    public static TResult Run<TResult>(Func<TResult> fn)
    {
        if (Disabled) return fn();
        return Task.Run(fn).GetAwaiter().GetResult();
    }
}
