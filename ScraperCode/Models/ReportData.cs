using System.Text.RegularExpressions;
using DbWebScraper.Models;


namespace ScraperCode.Models;

public class ReportData
{
    public static List<Link> Get(List<hostPageLinkErrorsQry> rs)
    {
        var links = new List<Link>();
        Link? link = null;
        var lastScapeId = 0;
        foreach (var item in rs)
        {
            if (item.scrapeId != lastScapeId)
            {
                if (link != null)
                {
                    links.Add(link);
                }

                link = new Link
                {
                    Id = item.id,
                    RawLink = item.rawLink,
                    ScrapeUri = item.scrapeUri,
                    StatusCode = item.statusCode,
                    Pages = []
                };
            }

            link?.Pages.Add(new Page
            {
                PageUrl = item.pageUri,
                OuterHtml = item.outerHtml
            });
            lastScapeId = item.scrapeId;
        }

        if (link != null)
        {
            links.Add(link);
        }

        return RemoteProtectedLinks(links);
    }

    public static List<Link> RemoteProtectedLinks(List<Link> linkArr)
    {
        return linkArr.Where(link => IsLinkNotProtected(link.RawLink)).ToList();
    }

    public static bool IsLinkNotProtected(string link)
    {
        var regs = new List<string>
        {
            "yelp.com",
            "linkedin.com",
            @"\.pdf"
        };
        return regs.All(reg => !Regex.IsMatch(link, reg, RegexOptions.IgnoreCase));
    }


    public class Link
    {
        public int Id { get; set; }
        public string StatusCode { get; set; }
        public string RawLink { get; set; }
        public string ScrapeUri { get; set; }
        public List<Page> Pages { get; set; } = new();
    }

    public class Page
    {
        public string PageUrl { get; set; }
        public string OuterHtml { get; set; }
    }
}