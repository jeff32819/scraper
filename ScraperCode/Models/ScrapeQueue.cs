using DbWebScraper.Models;

namespace ScraperCode.Models;

public class ScrapeQueue
{
    public int QueueCount { get; set; }

    /// <summary>
    ///     PageId only used for pages, not links
    /// </summary>
    public QueueItemModel QueueItem { get; set; }


    public class QueueItemModel
    {
        public QueueItemModel()
        {
            // queue is empty
        }

        public QueueItemModel(scrapeQueueQry rs)
        {
            scrapeId = rs.scrapeId;
            pageId = rs.pageId ?? 0;
            hostPageCount = rs.hostPageCount ?? 0;
            hostMaxPagesToScrape = rs.hostMaxPagesToScrape ?? 0;
            fileExt = rs.fileExt;
            host = rs.host;
            absoluteUri = rs.absoluteUri;
            headStatusCode = rs.headStatusCode;
            statusCode = rs.statusCode;
        }


        public int scrapeId { get; set; }

        public int pageId { get; set; }

        public int hostPageCount { get; set; }

        public int hostMaxPagesToScrape { get; set; }

        public string fileExt { get; set; }

        public string host { get; set; }

        public string absoluteUri { get; set; }

        public string headStatusCode { get; set; }

        public string statusCode { get; set; }

        public bool CheckIfSameHost(string link)
        {
            var uri = new Uri(link);
            return string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase);
        }
    }
}