using System.Text.RegularExpressions;
using Reports.Models;


namespace ScraperCode.Models;

public class ReportData
{
    public static BrokenLinkModel Get(List<DbScraper02.Models.hostPageLinkErrorsQry> rs)
    {
        var rv = new Reports.Models.BrokenLinkModel();
        rv.Links = new List<Reports.Models.BrokenLinkModel.LinkModel>();
        Reports.Models.BrokenLinkModel.LinkModel? link = null;
        var lastScapeId = 0;
        foreach (var item in rs)
        {
            if (item.scrapeId != lastScapeId)
            {
                if (link != null)
                {
                    rv.Links.Add(link);
                }

                link = new Reports.Models.BrokenLinkModel.LinkModel
                {
                    Id = item.id,
                    RawLink = item.rawLink,
                    ScrapeUri = item.scrapeCleanLink,
                    StatusCode = item.statusCode,
                    Pages = []
                };
            }

            link?.Pages.Add(new Reports.Models.BrokenLinkModel.Page
            {
                PageUrl = item.pageCleanLink,
                OuterHtml = item.outerHtml
            });
            lastScapeId = item.scrapeId;
        }

        if (link != null)
        {
            rv.Links.Add(link);
        }
        rv.Links = RemoteProtectedLinks(rv.Links);
        return rv;
    }

    public static List<Reports.Models.BrokenLinkModel.LinkModel> RemoteProtectedLinks(List<Reports.Models.BrokenLinkModel.LinkModel> linkArr)
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
}