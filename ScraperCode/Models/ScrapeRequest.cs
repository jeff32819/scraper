namespace ScraperCode.Models;

public class ScrapeRequest : IScrapeRequest
{

    public bool HtmlFromCache { get; set; }
    public string? Url { get; set; } = null!;
    public string Html { get; set; } = null!;
    public string StatusCode { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; } = null!;
    public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; } = null!;
    public bool WasRedirected { get; set; } = false;
    public string RedirectedFromUrl { get; set; } = "";
    public int RedirectStatusCode { get; set; }
}