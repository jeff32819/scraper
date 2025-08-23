using Microsoft.AspNetCore.Components;

namespace Reports
{
    public static class Code
    {
        public static MarkupString HtmlEncode(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return new MarkupString();
            }

            return new MarkupString(html.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;"));
        }
    }
}
