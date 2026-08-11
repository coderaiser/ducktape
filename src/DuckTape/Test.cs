namespace DuckTape;

public static class Test
{
    static readonly List<TestDefinition> _tests = new();

    public static void Run(string message, Func<T, Task> fn, bool skip = false, bool only = false) =>
        Add(message, fn, skip, only);

    public static void Run(string message, Action<T> fn, bool skip = false, bool only = false) =>
        Add(message, t => { fn(t); return Task.CompletedTask; }, skip, only);

    public static void Only(string message, Action<T> fn) => Add(message, t => { fn(t); return Task.CompletedTask; }, false, true);

    public static void Skip(string message, Action<T> fn) => Add(message, t => { fn(t); return Task.CompletedTask; }, true, false);

    static void Add(string message, Func<T, Task> fn, bool skip, bool only) =>
        _tests.Add(new TestDefinition(message, fn, skip, only, CallerAt()));

    internal static List<TestDefinition> All => _tests;

    static string CallerAt([System.Runtime.CompilerServices.CallerFilePath] string file = "",
                           [System.Runtime.CompilerServices.CallerLineNumber] int line = 0) =>
        $"{file}:{line}";
}
