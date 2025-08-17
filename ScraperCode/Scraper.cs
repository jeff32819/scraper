using CodeBase;
using ScraperCode.Models;

namespace ScraperCode;

public class Scraper
{
    public Scraper(DbService dbSvc, NLogger log)
    {
        DbSvc = dbSvc;
        Log = log;
    }
 
    private NLogger Log { get; }
    public DbService DbSvc { get; }

    public void PageSeedsAsync(List<string> urlList, string filePath, int maxPageToScrape)
    {
        var category = Path.GetFileNameWithoutExtension(filePath);
        foreach (var url in urlList)
        {
            Log.Info($"Category: {category}; Adding Seed: {url}");
            var logic = new ScrapeLogic(DbSvc);
            logic.AddHostSeed(url, maxPageToScrape, category);
        }
        File.Delete(filePath);
    }

    private static string FormatNumberCommaAndLeadingZero(int num) => num < 1000
                                                                    ? $"___,{num.ToString().PadLeft(3, '_')}"
                                                                    : num.ToString("N0").ToString().PadLeft(7, '_');

    public async Task ProcessScrapeHtml()
    {
        Log.Info($"ProcessScrapeHtml ~ START");
        while (DbSvc.ScrapeQueue() is { QueueCount: > 0 } queueItem)
        {
            Log.Info($"{FormatNumberCommaAndLeadingZero(queueItem.QueueCount)} LEFT ::  {queueItem.QueueItem.absoluteUri}");
            var headersOnly = queueItem.QueueItem.pageId == 0;

            Log.Info($"Getting  :: {queueItem.QueueItem.absoluteUri}");
            Log.Info($"Headers only = {headersOnly}");

            var response = await WebRequest.Parse(WebRequest.ScrapeMethod.HttpClient, queueItem.QueueItem.absoluteUri, headersOnly);
            Log.Info($"Response :: {response.StatusCode}");
            DbSvc.ScrapeUpdateSuccess(queueItem.QueueItem.scrapeId, response);
            if (headersOnly)
            {
                Log.Info($"Headers ONLY -- not adding links", 1);
                continue;
            }
            if (response.StatusCode.Equals("Forbidden", StringComparison.OrdinalIgnoreCase))
            {
                Log.Info("Response was Forbidden, not parsing links");
                continue;
            }
            await AddLinksFromPage(queueItem.QueueItem, response);


            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    DbSvc.ScrapeUpdateHtmlFail(queueItem.QueueItem.scrapeId, ex);
            //}

            if (!Code.EscKeyPressed())
            {
                continue;
            }
            Log.Info($"ProcessScrapeHtml ~ ESC key pressed ~ DONE");
            break;
        }
        Log.Info($"ProcessScrapeHtml ~ DONE");
    }


    /// <summary>
    /// </summary>
    /// <param name="queueItem"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    private async Task AddLinksFromPage(ScrapeQueue.QueueItemModel queueItem, IScrapeRequest response)
    {
        var maxLinksPerPage = 200;

        var logic = new ScrapeLogic(DbSvc);
        var parsedLinks = await Code.GetLinksWithHtmlAsync(queueItem.pageId, queueItem.absoluteUri, response.Html);
        DbSvc.PageUpdateLinkCount(queueItem.pageId, parsedLinks.Links.Count);

        if(parsedLinks.Links.Count > maxLinksPerPage)
        {
            Log.Info($"Not adding links (has {parsedLinks.Links.Count} links over the {maxLinksPerPage} limit)", 1);
            return;
        }

        DbSvc.LinksDeleteForPage(queueItem.pageId);
        Log.Info($"Links START -- TOTAL = {parsedLinks.Links.Count}", 1);
        foreach (var link in parsedLinks.Links)
        {
            var index = link.indexOnPage ?? 0; // is nullable for now, backwards compatible.
            var num = index + 1;
            Log.Info($"Page = {queueItem.pageId}; Link {Jeff32819DLL.MiscCore20.Code.PercentDoneToString(num, parsedLinks.Links.Count)} (#{num}) -- {link.absoluteUri}", 1);
            logic.AddLink(link);
            if(queueItem.CheckIfSameHost(link.absoluteUri))
            {
                logic.AddPage(link.absoluteUri);
            }
        }
        Log.Info($"Links END", 1);
    }
}