namespace ScraperCode.Models;

public class ScrapeRequest : IScrapeRequest
{
    public bool HtmlFromCache { get; set; }
    public string Url { get; set; }
    public string Html { get; set; }
    public string StatusCode { get; set; }
    public string ContentType { get; set; }
    public bool HeadersOnly { get; set; }
    public Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; }
    public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; }
}