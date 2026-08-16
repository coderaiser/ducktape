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
