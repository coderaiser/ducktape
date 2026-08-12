using System.Text;
using System.Text.RegularExpressions;

namespace DuckTape;

public static class Glob
{
    public static List<string> Expand(string pattern)
    {
        var normalized = Normalize(pattern);
        var root = StaticRoot(normalized);
        if (root.Length == 0) root = ".";
        if (!Directory.Exists(root)) return new();

        var prefix = root == "." ? "" : root + "/";
        var tail = root == "." ? normalized : normalized.Substring(root.Length).TrimStart('/');
        var regex = new Regex(BuildRegex(tail), RegexOptions.Compiled);

        var results = new List<string>();
        foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var nf = Normalize(f);
            var rel = root == "."
                ? nf
                : nf.StartsWith(prefix, StringComparison.Ordinal) ? nf.Substring(prefix.Length) : null;
            if (rel is null || !regex.IsMatch(rel)) continue;
            results.Add(prefix + rel);
        }
        results.Sort(StringComparer.Ordinal);
        return results;
    }

    static string Normalize(string path) => path.Replace('\\', '/');

    static string StaticRoot(string pattern)
    {
        int firstWildcard = -1;
        for (int i = 0; i < pattern.Length; i++)
        {
            if (pattern[i] == '*' || pattern[i] == '?') { firstWildcard = i; break; }
        }
        if (firstWildcard < 0) return pattern;
        var lastSlash = pattern.LastIndexOf('/', firstWildcard);
        return lastSlash < 0 ? "" : pattern.Substring(0, lastSlash);
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
                    {
                        sb.Append("(?:.*/)?");
                        i = j;
                    }
                    else
                    {
                        sb.Append(".*");
                        i = j - 1;
                    }
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
