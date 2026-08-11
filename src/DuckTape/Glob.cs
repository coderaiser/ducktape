using System.Text;
using System.Text.RegularExpressions;

namespace DuckTape;

public static class Glob
{
    public static List<string> Expand(string pattern)
    {
        var normalized = pattern.Replace('\\', '/');
        var root = StaticRoot(normalized);
        if (root.Length == 0) root = ".";
        if (!Directory.Exists(root)) return new();

        var regex = new Regex(BuildRegex(normalized), RegexOptions.Compiled);
        var results = Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(f => regex.IsMatch(Normalize(f)))
            .ToList();
        results.Sort(StringComparer.Ordinal);
        return results;
    }

    static string Normalize(string path) => path.Replace('\\', '/');

    static string StaticRoot(string pattern)
    {
        var sb = new StringBuilder();
        foreach (var s in pattern.Split('/'))
        {
            if (s.Length == 0) continue;
            if (s.Contains('*') || s.Contains('?')) break;
            if (sb.Length > 0) sb.Append('/');
            sb.Append(s);
        }
        return sb.ToString();
    }

    static string BuildRegex(string pattern)
    {
        var sb = new StringBuilder("^");
        var chars = pattern.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (c == '*')
            {
                int j = i;
                while (j < chars.Length && chars[j] == '*') j++;
                var n = j - i;
                if (n >= 2)
                {
                    if (j < chars.Length && chars[j] == '/')
                        sb.Append("(?:.*/)?");
                    else
                        sb.Append(".*");
                    i = j - 1;
                }
                else
                {
                    sb.Append("[^/]*");
                }
            }
            else if (c == '?')
            {
                sb.Append("[^/]");
            }
            else
            {
                sb.Append(Regex.Escape(c.ToString()));
            }
        }
        sb.Append('$');
        return sb.ToString();
    }
}
