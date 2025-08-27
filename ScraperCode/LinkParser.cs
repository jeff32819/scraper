using AngleSharp;
using AngleSharp.Dom;
using DbScraper02.Models;
using Jeff32819DLL.MiscCore20;

namespace ScraperCode;

public class LinkParser(scrapeQueueSpResult scrapeQueueQry, string scrapedLink)
{
    public scrapeQueueSpResult ScrapeQueueQry { get; set; } = scrapeQueueQry;
    public string ScrapedLink { get; set; } = scrapedLink;
    public Dictionary<string, pageTbl> PageDic { get; set; } = [];
    public List<linkTbl> LinkArr { get; set; } = [];

    public async Task Init(string html, int pageId)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));
        var indexOnPage = 0;
        var links = document.QuerySelectorAll("a");


        foreach (var item in links)
        {
            var linkParsed = new LinkItem(scrapeQueueQry, ScrapedLink, indexOnPage++, item, pageId);
            LinkArr.Add(linkParsed.Link);
            if (!linkParsed.Link.cleanLink.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            var pg = new pageTbl
            {
                host = linkParsed.UriSections.Uri.Host,
                cleanLink = linkParsed.Link.cleanLink,
                fullLink = linkParsed.Link.fullLink
            };
            PageDic.TryAdd(pg.cleanLink, pg);
        }
    }


    public class LinkItem
    {
        public LinkItem(scrapeQueueSpResult scrapeQueueQry, string scrapedLink, int indexOnPage, IElement a, int pageId)
        {
            Link = new linkTbl
            {
                pageId = pageId,
                host = "",
                indexOnPage = indexOnPage,
                outerHtml = a.OuterHtml,
                innerHtml = a.InnerHtml,
                cleanLink = "",
                fullLink = "",
                rawLink = "",
                addedDateTime = DateTime.UtcNow,
                errorMessage = ""
            };

            try
            {
                var href = a.GetAttribute("href");
                if (href == null)
                {
                    Link.errorMessage = "href is null for link tag";
                    return;
                }

                Link.rawLink = href;
                UriSections = new UriSections(scrapedLink, href);
                if (!UriSections.IsValid)
                {
                    return;
                }

                Link.host = UriSections.Uri.Host;
                Link.fullLink = UriSections.SchemeHostPathQueryFragment;
                Link.cleanLink = Code.CalcAbsoluteUri(UriSections);

            }
            catch (Exception ex)
            {
                Link.errorMessage = ex.Message;
            }
        }


        public linkTbl Link { get; }
        public UriSections UriSections { get; }
    }
}