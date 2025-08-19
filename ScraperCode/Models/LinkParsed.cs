

using DbWebScraper.Models;

namespace ScraperCode.Models;

public class LinkParsed
{
    public List<linkTbl> Links { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}