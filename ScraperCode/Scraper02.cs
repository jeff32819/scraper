using CodeBase;
using DbScraper02.Models;
using Newtonsoft.Json;

// ReSharper disable InvertIf

namespace ScraperCode;

public static class Scraper02
{
    private const int PageMaxLinkCount = 200;


    private static async Task<LinkParser> GetLinkParser(scrapeQueueQry scrapeQueueQry, string scrapedLink, string html, int pageId)
    {
        var linkParser = new LinkParser(scrapeQueueQry, scrapedLink);
        await linkParser.Init(html, pageId);
        return linkParser;
    }

    public static async Task Process(DbService02 dbSvc, NLogger setupLogger)
    {
        while (dbSvc.ScrapeQueue() is { QueueCount: > 0 } queueItem)
        {
            var scrape = await ScrapeWebAndUpdate(dbSvc, queueItem);
            var page = dbSvc.PageLookup(scrape);
            if (page == null || scrape.html == null) // html is null ususally if redirected.
            {
                continue;
            }

            var linkParser = await GetLinkParser(queueItem.QueueItem, scrape.cleanLink, scrape.html, page.id);
            await dbSvc.LinksDeleteForPage(page.id);

            switch (linkParser.LinkArr.Count)
            {
                case 0:
                    await dbSvc.UpdateLinkCount(page, 0);
                    continue; // no links found, skip this page
                case > PageMaxLinkCount:
                    await dbSvc.UpdateLinkCountOverLimit(page, linkParser.LinkArr.Count, PageMaxLinkCount);
                    continue; // too many links, skip this page
            }

            foreach (var link in linkParser.LinkArr)
            {
                Console.WriteLine($"Adding link: {JsonConvert.SerializeObject(link, Formatting.Indented)}");
                await dbSvc.LinkAdd(page, link);
            }

            foreach (var kvp in linkParser.PageDic)
            {
                var host = await dbSvc.HostManager.Lookup(kvp.Value.host);
                if (host.maxPageToScrape < 0)
                {
                    continue;
                }

                Console.WriteLine($"Adding page: {kvp.Value.cleanLink}");
                await dbSvc.PageAdd(kvp.Value.host, kvp.Value.fullLink, kvp.Value.cleanLink);
            }

            await dbSvc.UpdateLinkCount(page, linkParser.LinkArr.Count);
        }
    }

    public static async Task<scrapeTbl> ScrapeWebAndUpdate(DbService02 dbSvc, DbService02.ScrapeQueueModel queueItem)
    {
        if (queueItem.QueueItem.statusCode != -1) // already done
        {
            return dbSvc.DbCtx.scrapeTbl.Single(x => x.id == queueItem.QueueItem.scrapeId);
        }

        Console.WriteLine(queueItem.QueueItem.cleanLink);

        var uri = new Uri(queueItem.QueueItem.cleanLink);

        if (!uri.IsAbsoluteUri)
        {
            var errorMessage = $"NotAbsoluteUri: {uri.ToString()}";
            return await dbSvc.ScrapeErrorMessage(queueItem.QueueItem, 0, errorMessage);
        }

        var tmp = await HttpClientHelper.GetHttpClientResponse(uri);
        if (tmp.StatusCode == 0)
        {
            return await dbSvc.ScrapeErrorMessage(queueItem.QueueItem, tmp.HttpClientResponse.StatusCode, tmp.ErrorMessage);
        }

        if (tmp.IsRedirected)
        {
            var errorMessage = $"Redirected: {tmp.HttpClientResponse.RedirectedLocation}";
            return await dbSvc.ScrapeErrorMessage(queueItem.QueueItem, tmp.HttpClientResponse.StatusCode, errorMessage);
        }

        return await dbSvc.ScrapeUpdateHtml(queueItem.QueueItem, tmp);
    }
}