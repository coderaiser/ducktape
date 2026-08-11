namespace DuckTape;

public static class Format
{
    public static string AddSpaces(string s) => $"      {s}";

    public static string FormatOutput(string s) =>
        string.Join('\n', s.Split('\n').Select(AddSpaces));
}
