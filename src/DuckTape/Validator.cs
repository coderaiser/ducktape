namespace DuckTape;

public record ValidationResult(string? Message, string? At);

public class Validator
{
    static readonly System.Text.RegularExpressions.Regex _scopeRe =
        new(@"^[\w\-\/\d\s]+:.*", System.Text.RegularExpressions.RegexOptions.Compiled);

    readonly List<TestDefinition> _tests;
    readonly HashSet<string> _seen = new();

    public Validator(List<TestDefinition> tests) => _tests = tests;

    public ValidationResult Validate(string message, int assertionsCount)
    {
        if (CheckDuplicates())
        {
            var dupes = _tests.Where(t => t.Message == message).ToList();
            if (dupes.Count > 1 && _seen.Add(message))
                return new($"Duplicate: {message}", dupes[1].At);
        }

        if (CheckScopes())
        {
            if (!_scopeRe.IsMatch(message))
                return new($"Scope required: 'scope: subject', got: '{message}'", null);
        }

        if (CheckAssertionsCount())
        {
            if (assertionsCount == 0)
                return new("Only one assertion per test allowed, looks like you have none", null);
            if (assertionsCount > 1)
                return new("Only one assertion per test allowed, looks like you have more", null);
        }

        return new(null, null);
    }

    static bool CheckDuplicates() =>
        Environment.GetEnvironmentVariable("DUCKTAPE_CHECK_DUPLICATES") != "0";

    static bool CheckScopes() =>
        Environment.GetEnvironmentVariable("DUCKTAPE_CHECK_SCOPES") == "1";

    static bool CheckAssertionsCount() =>
        Environment.GetEnvironmentVariable("DUCKTAPE_CHECK_ASSERTIONS_COUNT") != "0";
}
