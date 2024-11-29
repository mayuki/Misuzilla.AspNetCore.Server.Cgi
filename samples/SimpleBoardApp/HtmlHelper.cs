public static class HtmlHelper
{
    public static string ToHtmlEscapedString(this string s, bool newLineAsBr = false)
        => newLineAsBr
            ? s.Replace("&", "&amp;").Replace("<", "&lt;").Replace("\"", "&quot;").Replace("\n", "<br />")
            : s.Replace("&", "&amp;").Replace("<", "&lt;").Replace("\"", "&quot;");
}