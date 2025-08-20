using System.Text;
using ScraperCode.Models;

namespace ScraperCode;

public static class ScrapeReport
{
    private static string HtmlEncode(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        return html.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    public static async Task Process(DbService02 dbSvc, string url)
    {
        var uri = new Uri(url);

        var sb = new StringBuilder();
        sb.AppendLine("<style>");
        sb.AppendLine(".green-button {");
        sb.AppendLine("    background-color: #28a745; /* Bootstrap green */");
        sb.AppendLine("    color: white;");
        sb.AppendLine("    border: none;");
        sb.AppendLine("    padding: 8px 16px;");
        sb.AppendLine("    border-radius: 4px;");
        sb.AppendLine("    font-size: 1em;");
        sb.AppendLine("    cursor: pointer;");
        sb.AppendLine("}");
        sb.AppendLine(".green-button:hover {");
        sb.AppendLine("    background-color: #218838; /* Darker green on hover */");
        sb.AppendLine("}");
        sb.AppendLine(".red-button {");
        sb.AppendLine("    background-color: #dc3545; /* Bootstrap red */");
        sb.AppendLine("    color: white;");
        sb.AppendLine("    border: none;");
        sb.AppendLine("    padding: 8px 16px;");
        sb.AppendLine("    border-radius: 4px;");
        sb.AppendLine("    font-size: 1em;");
        sb.AppendLine("    cursor: pointer;");
        sb.AppendLine("}");
        sb.AppendLine(".red-button:hover {");
        sb.AppendLine("    background-color: #b52a37; /* Darker red on hover */");
        sb.AppendLine("}");
        sb.AppendLine("</style>");
        sb.AppendLine("<script>");
        sb.AppendLine("function removeElementById(elementId) {");
        sb.AppendLine("  const element = document.getElementById(elementId);");
        sb.AppendLine("  if (element) {");
        sb.AppendLine("    element.parentNode.removeChild(element);");
        sb.AppendLine("  } else {");
        sb.AppendLine("    console.warn(`Element with ID [${elementId} not found.`); ");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine("function removeAllButtonBars(button) {");
        sb.AppendLine("  const bars = document.querySelectorAll('.button-bar');");
        sb.AppendLine("  bars.forEach(bar => bar.parentNode.removeChild(bar));");
        sb.AppendLine("  button.parentNode.removeChild(button);");
        sb.AppendLine("}");
        sb.AppendLine("</script>");
        sb.AppendLine("<h3 style=\"font-family:arial;\">Broken Links</h3>");


        var rs = dbSvc.ReportPagesForDomain(uri);
        var data = ReportData.Get(rs);

        foreach (var link in data)
        {
            sb.AppendLine("");
            sb.AppendLine($"<!-- {link.RawLink} -->");
            sb.AppendLine("");
            sb.AppendLine($"<div id=\"row_{link.Id}\" style=\"background-color:#ebf5fb; color:black; font-family:arial;margin-bottom:10px;border:solid 1px blue; padding:10px;border-radius:5px;\">");
            sb.AppendLine($"    <div id=\"buttons_{link.Id}\" class=\"button-bar\" style=\"margin-bottom:10px;\">");
            sb.AppendLine($"        <button type=\"button\" class=\"red-button\"   onclick=\"removeElementById('row_{link.Id}')\">Remove</button>");
            sb.AppendLine("    </div>");
            sb.AppendLine($"    <div style=\"margin-bottom:3px;\">Link: <a href=\"{link.ScrapeUri}\" target=\"_blank\" style=\"color:blue;font-weight:bolder;\">{link.RawLink}</a></div>");
            // sb.AppendLine($"    <div style=\"margin-bottom:3px;\">Status Code: <span style=\"color:black;font-weight:bolder;\">{link.StatusCode}</span></div>");

            foreach (var pg in link.Pages)
            {
                sb.AppendLine($"    <div style=\"margin-top:15px; margin-bottom:3px;\">On Page: <a href=\"{pg.PageUrl}\" target=\"_blank\" style=\"color:blue;font-weight:bolder;\">{pg.PageUrl}</a></div>");
                sb.AppendLine("    <div style=\"margin-top:5px;border:solid 1px blue; background-color:#fff; color:black;padding:10px;border-radius:5px; font-family:lucida console,courier new;font-size:small;\">");
                sb.AppendLine("        <div style=\"font-weight:bolder;font-size:small;\">Outer HTML:</div>");
                sb.Append($"            <div>{HtmlEncode(pg.OuterHtml)}</div>");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine("</div>");
        }

        sb.AppendLine("<div style=\"margin-top:25px;\"></div>");
        sb.AppendLine("<div><button class=\"red-button\" onclick=\"removeAllButtonBars(this)\">Remove All Buttons</button></div>");
        sb.AppendLine("<div style=\"margin-top:25px;\"></div>");
        sb.AppendLine("<h1 style=\"color:white;margin-bottom:50px;\">&nbsp;&nbsp;&nbsp;</h1>");
        sb.AppendLine($"<h1 style=\"color:white;font-family:lucida console;courier new;\">{uri}</h1>");

        const string folder = "t:\\bad-link-reports";
        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(Path.Combine(folder, $"{uri.Host}.html"), sb.ToString());


        //sb.AppendLine($"<h1></h1>");
        //sb.AppendLine($"<h1></h1>");
    }
}