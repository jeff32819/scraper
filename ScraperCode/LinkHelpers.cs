using System.Diagnostics;
using System.Text.RegularExpressions;
using Jeff32819DLL.HtmlParser;
using Jeff32819DLL.MiscCore20;
using ScraperCode.Models;

namespace ScraperCode;

public class LinkHelpers
{
    public static async Task<List<string>> Parse(string html, Log? log = null)
    {
        var document = await AngleSharpHelper.LoadHtmlFromStringAsync(html);
        var arr = AngleSharpHelper.GetLinks(document).ToList();

        if (log == null)
        {
            return arr;
        }

        log.JsonToFile(arr);
        return arr;
    }

    public static async Task<List<string>> Parse(DomainConfig domain, string html, Log? log = null)
    {
        var links = await Parse(html, log);
        var arr = new List<string>();
        foreach (var tmp in links.Select(link => LinkToAbsolutePath(domain, link)))
        {
            Debug.Print(tmp.OriginalValue);
            Debug.Print(tmp.CurrentValue);
            Debug.Print("");
            arr.Add(tmp.CurrentValue);
        }

        return arr;
    }

    public static LinkPath LinkToAbsolutePath(DomainConfig domainRoot, string url)
    {
        if (url.StartsWith("//")) // external link, do nothing.
        {
            return new LinkPath(url);
        }

        if (Regex.IsMatch(url, @"^(http|https|mailto|ftp|file|tel)\:", RegexOptions.IgnoreCase))
        {
            return new LinkPath(url);
        }

        if (url.StartsWith("./"))
        {
            url = url.TrimStart('.'); // https://screamscape.com/ has links that start with ./link.html and appears valid, but weird.
        }

        var result = new LinkPath(url);
        result.CurrentValue = Regex.Replace(result.CurrentValue, @"^[\.]+\/", "/");
        // Debug.Print($"[{result.CurrentValue}].StartsWith(\"/\") = {result.CurrentValue.StartsWith("/")}");
        if (result.CurrentValue.StartsWith("/"))
        {
            result.CurrentValue = $"{domainRoot.DomainFullRoot}{result.CurrentValue}";
        }

        result.CurrentValue = result.CurrentValue.TrimEnd('/');
        return result;
    }

    public static bool CalcIsInternalPage(DomainConfig parentDomain, string child)
    {
        var childDomain = new DomainConfig(child);
        Debug.Print("child  = " + childDomain.Domain);
        Debug.Print("parent = " + parentDomain.Domain);
        Debug.Print("");
        return string.Equals(childDomain.Domain, parentDomain.Domain, StringComparison.CurrentCultureIgnoreCase);
    }
}