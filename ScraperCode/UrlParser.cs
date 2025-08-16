// ReSharper disable MemberCanBePrivate.Global

namespace ScraperCode;

/// <summary>
/// https://learn.microsoft.com/en-us/dotnet/api/system.uri?view=net-9.0
/// </summary>
public static class UrlParser
{
    /// <summary>
    ///     URL parser, that will add page host info to the linkUrl if it is a relative URL.
    /// </summary>
    /// <param name="pageUrl">Must be a FULL url including http</param>
    /// <param name="linkUrl">Can be a relative url</param>
    public static Uri GetUri(string pageUrl, string linkUrl)
    {
        return GetUri(new Uri(pageUrl), linkUrl);
    }
    public static Uri GetUri(Uri pageUri, string linkUrl)
    {
        var uri = new Uri(linkUrl, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
        {
            uri = new Uri(pageUri, linkUrl);
        }
        return uri;
    }
}