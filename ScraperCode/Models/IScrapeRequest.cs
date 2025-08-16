namespace ScraperCode.Models;

public interface IScrapeRequest
{
    bool HtmlFromCache { get; set; }
    public string Url { get; set; }
    public string Html { get; set; }
    public string StatusCode { get; set; }
    string ContentType { get; set; }
    bool HeadersOnly { get; set; }
    Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; }
    Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; }
}