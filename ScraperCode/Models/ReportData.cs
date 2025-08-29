using System.Text.RegularExpressions;
using DbScraper02.Models;
using Reports.Models;

namespace ScraperCode.Models;

/// <summary>
/// Report data processing
/// </summary>
public abstract class ReportData
{
    /// <summary>
    /// Constructs a <see cref="BrokenLinkModel"/> from a list of link error query results.
    /// </summary>
    /// <remarks>This method processes the input list to group link errors by their scrape ID, creating a
    /// hierarchical  structure where each broken link is associated with its corresponding pages. The resulting model
    /// is  further processed to remove protected links before being returned.</remarks>
    /// <param name="rs">A list of <see cref="hostPageLinkErrorsQry"/> objects representing link error data.  Each item contains
    /// information about a broken link, including its associated pages and metadata.</param>
    /// <returns>A <see cref="BrokenLinkModel"/> containing a collection of broken links and their associated pages.</returns>
    public static BrokenLinkModel Get(List<hostPageLinkErrorsQry> rs)
    {
        var rv = new BrokenLinkModel
        {
            Links = []
        };
        BrokenLinkModel.LinkModel? link = null;
        var lastScapeId = 0;
        foreach (var item in rs)
        {
            if (item.scrapeId != lastScapeId)
            {
                if (link != null)
                {
                    rv.Links.Add(link);
                }

                link = new BrokenLinkModel.LinkModel
                {
                    Id = item.id,
                    RawLink = item.rawLink,
                    ScrapeUri = item.scrapeCleanLink,
                    StatusCode = item.statusCode,
                    Pages = []
                };
            }

            link?.Pages.Add(new BrokenLinkModel.Page
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
    /// <summary>
    /// Remote protected links such as yelp, linkedin, pdfs
    /// </summary>
    /// <param name="linkArr"></param>
    /// <returns></returns>
    private static List<BrokenLinkModel.LinkModel> RemoteProtectedLinks(List<BrokenLinkModel.LinkModel> linkArr)
    {
        return linkArr.Where(link => IsLinkNotProtected(link.RawLink)).ToList();
    }
    /// <summary>
    /// Is link not protected such as yelp, linkedin, pdfs
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    private static bool IsLinkNotProtected(string link)
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