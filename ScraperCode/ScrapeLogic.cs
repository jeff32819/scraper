using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using ScraperCode.DbCtx;

namespace ScraperCode;

public class ScrapeLogic
{
    public ScrapeLogic(DbService db)
    {
        Db = db;
    }

    private DbService Db { get; }
    private UriSections UriSections { get; set; }


    private linkTbl Link { get; set; }

    /// <summary>
    /// Adding a page happens when a scrape is searched for and the host is being monitored.
    /// </summary>
    /// <param name="link"></param>
    public void AddPage(string link)
    {
        UriSections = new UriSections(link);
        var host = Db.HostGet(UriSections.Uri); // if not exists, it will throw an exception
        var scrape = Db.ScrapeAdd(GetScrapeObj());
        Db.PageAdd(GetPageObj(host.id, scrape.id));
    }

    /// <summary>
    /// Fix -- add pages that are in scrape table that the host should be monitored.
    /// </summary>
    /// <param name="scrape"></param>
    public void AddPage(scapeLinksThatShouldBeInPages scrape)
    {
        UriSections = new UriSections(scrape.absoluteUri);
        Db.PageAdd(GetPageObj(scrape.hostId, scrape.scrapeId));
    }
    /// <summary>
    /// Adding a link happens after a page has been scraped.
    /// </summary>

    public void AddLink(linkTbl link)
    {
        if (link.pageId == 0)
        {
            throw new Exception("page id should not be 0, is required");
        }
        UriSections = new UriSections(link.absoluteUri);
        link.absoluteUri = CalcAbsoluteUri();
        Db.HostAddIfNotExists(UriSections.Uri); // if not exists
        
        var skipLogic = SkipVerifyingLinkCalc(link.absoluteUri);
        if (skipLogic.ShouldSkip)
        {
            link.scrapeId = -1; // mark as skipped
            link.skipReason = skipLogic.Reason;
            Db.LinkAdd(link);
            return;
        }
        var scrape = Db.ScrapeAdd(GetScrapeObj()); // if not exists
        link.scrapeId = scrape.id;
        Db.LinkAdd(link); // always added, page links are deleted before adding new ones
    }


    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    private static SkipVerifyingLinkModel SkipVerifyingLinkCalc(string link)
    {
        if (string.IsNullOrEmpty(link))
        {
            return new SkipVerifyingLinkModel("BLANK_LINK");
        }
        link = link.Trim();
        if (Regex.IsMatch(link, "^http", RegexOptions.IgnoreCase))
        {
            return new SkipVerifyingLinkModel();
        }
        if (Regex.IsMatch(link, "about:blank", RegexOptions.IgnoreCase))
        {
            return new SkipVerifyingLinkModel("about:blank");
        }
        var colonIndex = link.IndexOf(':');
        if (colonIndex < 1)
        {
            return link.Length < 100 ? new SkipVerifyingLinkModel(link) : new SkipVerifyingLinkModel(link[..100]);
        }
        var valueBeforeColon = link[..colonIndex];
        return new SkipVerifyingLinkModel(valueBeforeColon);

    }

    public class SkipVerifyingLinkModel
    {
        public SkipVerifyingLinkModel()
        {
            ShouldSkip = false;
            Reason = string.Empty;
        }
        public SkipVerifyingLinkModel(string reason)
        {
            ShouldSkip = true;
            Reason = reason;
        }
        public bool ShouldSkip { get; }
        public string Reason { get; }
    }



    /// <summary>
    /// Add host only happens with a seeding a host from a list
    /// </summary>
    /// <param name="link"></param>
    /// <param name="maxPagesToScrape"></param>
    /// <param name="category"></param>
    public void AddHostSeed(string link, int maxPagesToScrape, string category)
    {
        UriSections = new UriSections(link);
        var host = Db.HostAddSeed(UriSections.Uri, maxPagesToScrape, category);
        var scrape = Db.ScrapeAdd(GetScrapeObj());
        Db.PageAdd(GetPageObj(host.id, scrape.id));
    }

    private scrapeTbl GetScrapeObj()
    {
        var fileExt = new FileExtExtractor(UriSections.Uri);

        return new scrapeTbl
        {
            fileExt = fileExt.Extension,
            host = UriSections.Uri.Host,
            absoluteUri = CalcAbsoluteUri(),
            addedDateTime = DateTime.UtcNow
        };
    }

    private pageTbl GetPageObj(int hostId, int scrapeId)
    {
        var fileExt = new FileExtExtractor(UriSections.Uri);
        return new pageTbl
        {
            hostId = hostId,
            scrapeId = scrapeId,
            fileExt = fileExt.Extension,
            scheme = UriSections.Uri.Scheme,
            host = UriSections.Uri.Host,
            path = UriSections.Uri.AbsolutePath,
            query = UriSections.Uri.Query,
            port = UriSections.Uri.Port,
            absoluteUri = CalcAbsoluteUri(),
            addedDateTime = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Trim last slash from absolute url, but only if it is not the first character.
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    private static string TrimLastSlash(string txt)
    {
        return string.IsNullOrEmpty(txt) || txt.LastIndexOf('/') == 0 ? txt : txt.TrimEnd('/');
    }
    private string CalcAbsoluteUri()
    {
        return UriSections.SchemeHostPathQuery.TrimEnd('/');
    }
}