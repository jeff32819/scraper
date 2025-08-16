using Jeff32819DLL.HtmlParser;

namespace ScraperCode.Models;

public class Link
{
    public Link(DomainConfig domain, string url)
    {
        UrlRaw = url;
        UrlFix = LinkHelpers.LinkToAbsolutePath(domain, url).CurrentValue;
        IsInternal = LinkHelpers.CalcIsInternalPage(domain, url);
        Protocol = CalcProtocol(url);
        UrlRaw = url;
        IsInternalPage = Code.CalcIsInternalPage(domain, UrlFix);
    }

    public string UrlRaw { get; set; }
    public string UrlFix { get; }
    public bool UrlWasCleaned => UrlRaw != UrlFix;

    public ScraperEnum.LinkProtocol Protocol { get; }

    public bool IsInternal { get; }


    public bool IsInternalPage { get; }


    /// <summary>
    ///     Calculate Protocol
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private ScraperEnum.LinkProtocol CalcProtocol(string url)
    {
        if (url.StartsWith("?"))
        {
            return ScraperEnum.LinkProtocol.http; // bringmeboxes has links that start with question mark for adding to cart.
        }

        url = url.ToLower().Trim();
        if (url.StartsWith("#"))
        {
            return ScraperEnum.LinkProtocol.http; // skip anchors
        }

        return url.StartsWith("mailto") ? ScraperEnum.LinkProtocol.mailto :
            url.StartsWith("ftp") ? ScraperEnum.LinkProtocol.ftp :
            url.StartsWith("file") ? ScraperEnum.LinkProtocol.file :
            url.StartsWith("tel") ? ScraperEnum.LinkProtocol.tel :
            url.StartsWith("javascript") ? ScraperEnum.LinkProtocol.javascript :
            ScraperEnum.LinkProtocol.http;
    }
}