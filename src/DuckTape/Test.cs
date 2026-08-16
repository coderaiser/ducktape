namespace DuckTape;

public static class Test
{
    static readonly List<TestDefinition> _tests = new();

    /// <summary>
    /// Returns a local test function. Usage: var test = CreateTest();
    /// Mirrors supertape's createTest() API.
    /// </summary>
    public static Action<string, Action<T>> CreateTest() =>
        (message, fn) => Add(message, t => { fn(t); return Task.CompletedTask; }, false, false);

    /// <summary>
    /// Async variant. Usage: var test = CreateTestAsync();
    /// </summary>
    public static Action<string, Func<T, Task>> CreateTestAsync() =>
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
