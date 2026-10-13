using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SolidShowdown.Catalog;

/// <summary>Reads the embedded example sources and turns them into highlighted HTML.</summary>
public static partial class SourceCode
{
    private static readonly ConcurrentDictionary<string, string> Cache = new();

    public static string Read(string path) => Cache.GetOrAdd(path, static p =>
    {
        var assembly = typeof(SourceCode).Assembly;
        // RecursiveDir uses the OS path separator, so normalise before comparing.
        var name = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.Replace('\\', '/').Equals(p, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Embedded example '{p}' not found.");
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Replace("\r\n", "\n").TrimEnd();
    });

    private static readonly HashSet<string> Keywords =
    [
        "abstract", "as", "base", "bool", "break", "case", "catch", "class", "const", "continue",
        "decimal", "default", "do", "double", "else", "enum", "false", "finally", "for", "foreach",
        "get", "if", "in", "init", "int", "interface", "internal", "is", "long", "namespace", "new",
        "null", "object", "out", "override", "private", "protected", "public", "readonly", "record",
        "required", "return", "sealed", "set", "static", "string", "struct", "switch", "this", "throw",
        "true", "try", "typeof", "using", "var", "virtual", "void", "when", "where", "while", "with",
        "yield", "async", "await", "params", "nameof", "char", "byte", "operator", "implicit", "explicit",
    ];

    [GeneratedRegex("""
        (?<comment>//[^\n]*|/\*[\s\S]*?\*/)
        |(?<string>\$?@?"(?:[^"\\\n]|\\.)*"|'(?:[^'\\\n]|\\.)')
        |(?<number>\b\d+(?:\.\d+)?[mMdDfFL]?\b)
        |(?<word>\b[A-Za-z_][A-Za-z0-9_]*\b)
        """, RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex Tokens();

    /// <summary>Returns one HTML string per source line.</summary>
    public static IReadOnlyList<string> HighlightLines(string source)
    {
        var html = new StringBuilder();
        var last = 0;
        foreach (Match m in Tokens().Matches(source))
        {
            html.Append(WebUtility.HtmlEncode(source[last..m.Index]));
            var text = m.Value;
            string? css = null;
            if (m.Groups["comment"].Success) css = CommentClass(text);
            else if (m.Groups["string"].Success) css = "tk-str";
            else if (m.Groups["number"].Success) css = "tk-num";
            else if (Keywords.Contains(text)) css = "tk-kw";
            else if (char.IsUpper(text[0])) css = "tk-type";

            if (css is null) html.Append(WebUtility.HtmlEncode(text));
            else
            {
                // Multi-line comments must be closed/re-opened per line so lines can be split safely.
                var encoded = WebUtility.HtmlEncode(text).Replace("\n", $"</span>\n<span class=\"{css}\">");
                html.Append($"<span class=\"{css}\">{encoded}</span>");
            }
            last = m.Index + m.Length;
        }
        html.Append(WebUtility.HtmlEncode(source[last..]));
        return html.ToString().Split('\n');
    }

    // Comment conventions used throughout the examples so the important ones pop out.
    private static string CommentClass(string comment)
    {
        if (comment.Contains('❌')) return "tk-comment tk-bad";
        if (comment.Contains('✅')) return "tk-comment tk-good";
        if (comment.Contains("CONTEXT")) return "tk-comment tk-context";
        return "tk-comment";
    }
}
