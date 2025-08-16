using Jeff32819DLL.HtmlParser.Models;

namespace ScraperCode.Models;

public class ScrapeResponse 
{
    public string Url { get; set; }
    public string StatusCode { get; set; }
    public bool HtmlFromCache { get; set; }
}