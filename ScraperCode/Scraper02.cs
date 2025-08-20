using AngleSharp;
using AngleSharp.Dom;

using CodeBase;

using DbScraper02.Models;

// ReSharper disable InvertIf

namespace ScraperCode
{
    public static class Scraper02
    {
        public static async Task Process(DbService02 dbSvc, NLogger setupLogger)
        {
            while (dbSvc.ScrapeQueue() is { QueueCount: > 0 } queueItem)
            {

                var linkUniqueTbl = await GetLinkUniqueTbl(dbSvc, queueItem);

                if (linkUniqueTbl.statusCode.IsRedirectStatusCode())
                {
                    continue; // already did this -- should not happen
                }
                var links = await ParseLinks(linkUniqueTbl.id, linkUniqueTbl.absoluteUri, linkUniqueTbl.html);
                const int maxLinks = 200;
                dbSvc.LinksDeleteForPage(linkUniqueTbl.id);
                if (links.Count > maxLinks)
                {
                    await dbSvc.UpdateLinkCountOverLimit(linkUniqueTbl, links.Count, maxLinks);
                    continue; // too many links, skip this page
                }
                foreach (var item in links)
                {
                    await dbSvc.LinkUniqueAddToDb(item.absoluteUri);
                    await dbSvc.LinksAddToDb(item);
                }
                await dbSvc.UpdateLinkCount(linkUniqueTbl, links.Count);
            }
        }

        public static async Task<linkUniqueTbl> GetLinkUniqueTbl(DbService02 dbSvc, DbService02.ScrapeQueueModel queueItem)
        {
            if (queueItem.QueueItem.statusCode != -1)
            {
                return dbSvc.DbCtx.linkUniqueTbl.Single(x => x.id == queueItem.QueueItem.id);
            }
            var uri = new Uri(queueItem.QueueItem.absoluteUri);
            var tmp = await HttpClientHelper.GetHttpClientResponse(uri);
            if (tmp.IsRedirected)
            {
                return await dbSvc.ScrapeUpdateRedirected(queueItem.QueueItem, tmp);
            }
            return await dbSvc.ScrapeUpdateHtml(queueItem.QueueItem, tmp);
        }
        public static async Task<List<linkTbl>> ParseLinks(int uniqueId, string absoluteUri, string html)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));
            var indexOnPage = 0;
            return document.QuerySelectorAll("a").Select(a => ParseEachLink(uniqueId,indexOnPage++, a, absoluteUri)).ToList();

        }

        private static linkTbl ParseEachLink(int uniqueId, int indexOnPage, IElement a, string absoluteUri)
        {
            var link = new linkTbl
            {
                uniqueId = uniqueId,
                indexOnPage = indexOnPage,
                outerHtml = a.OuterHtml,
                innerHtml = a.InnerHtml,
                addedDateTime = DateTime.UtcNow,
                errorMessage = ""
            };

            try
            {
                var href = a.GetAttribute("href");
                if (href == null)
                {
                    link.errorMessage = "href is null for link tag";
                    return link;
                }
                link.rawLink = href;
                var newUri = new ScraperCode.UriSections(absoluteUri, href);
                if (!newUri.IsValid)
                {
                    return link;
                }
                link.absoluteUri = ScraperCode.Code.CalcAbsoluteUri(newUri);
                return link;
            }
            catch (Exception ex)
            {
                link.errorMessage = ex.Message;
                return link;
            }
        }

    }
}
