using System.Text.RegularExpressions;

using Dapper;

using DbScraper02.Models;

using Jeff32819DLL.MiscCore20;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using ScraperCode.Models;

namespace ScraperCode;

public class DbService02
{
    public DbService02(Scraper02Context db)
    {
        DbCtx = db;
        DbConnString = DbCtx.Database.GetConnectionString() ?? throw new Exception("No database connection string");
        HostManager = new HostManager(db);
    }


    public HostManager HostManager { get; }
    public string DbConnString { get; }
    public Scraper02Context DbCtx { get; }

    public TimeoutRetry TimeoutRetry => new(3, 10000, "t:\\timeout-log.txt");

    public async Task StaticDataUpdate()
    {
        await DbCtx.Procedures.fileTypeAllowedToScrapeInsertSpAsync();
    }


    public ScrapeQueueModel ScrapeQueue()
    {
        TimeoutRetry.Reset();
        while (TimeoutRetry.Running)
        {
            try
            {
                var count = DbCtx.scrapeQueueQry.Count();
                if (count == 0)
                {
                    return new ScrapeQueueModel
                    {
                        QueueCount = 0,
                        QueueItem = new scrapeQueueQry()
                    };
                }

                var skip = new Random().Next(count);
                var rs = DbCtx.scrapeQueueQry.Skip(skip).Take(1).First();
                return new ScrapeQueueModel
                {
                    QueueCount = count,
                    QueueItem = rs
                };
            }
            catch (Exception ex) // when (ex is TimeoutException || ex.InnerException is TimeoutException)
            {
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine(ex.Message);
                Console.WriteLine("----------------------------------------------------");
                if (TimeoutRetry.RetriesMaxedOut)
                {
                    throw;
                }

                TimeoutRetry.Delay();
            }
        }

        throw new Exception("timeout");
    }

    /// <summary>
    ///     Maybe need to do more after it is redirected.
    /// </summary>
    /// <param name="queueItemQueueItem"></param>
    /// <param name="tmp"></param>
    public async Task<scrapeTbl> ScrapeErrorMessage(scrapeQueueQry queueItemQueueItem, int statusCode, string errorMessage)
    {
        var rs = DbCtx.scrapeTbl.Single(x => x.id == queueItemQueueItem.scrapeId);
        rs.statusCode = statusCode;
        rs.errorMessage = errorMessage;
        rs.scrapeDateTime = DateTime.UtcNow;
        await DbCtx.SaveChangesAsync();
        return rs;
    }
    public async Task PageAddAfterScraped(scrapeQueueQry queueItemQueueItem, HttpClientResponseContainer tmp)
    {
        var host = await HostManager.Lookup(queueItemQueueItem.host);
        if (host.maxPageToScrape < 0)
        {
            return;
        }

        if ((tmp.HttpClientResponse.ContentType ?? "unknown") != "text/html")
        {
            return;
        }
        await DbCtx.Procedures.pageAddSpAsync(host.host, queueItemQueueItem.cleanLink, queueItemQueueItem.cleanLink);
    }
    public async Task<scrapeTbl> ScrapeUpdateHtml(scrapeQueueQry queueItemQueueItem, HttpClientResponseContainer tmp)
    {
        await PageAddAfterScraped(queueItemQueueItem, tmp);
        var rs = DbCtx.scrapeTbl.Single(x => x.id == queueItemQueueItem.scrapeId);
        rs.statusCode = tmp.HttpClientResponse.StatusCode;
        rs.html = tmp.HttpClientResponse.Content ?? "";
        rs.contentType = tmp.HttpClientResponse.ContentType ?? "UNKNOWN";
        rs.responseHeaders = tmp.HttpClientResponse.ResponseHeadersToJson;
        rs.contentHeaders = tmp.HttpClientResponse.ContentHeadersToJson;
        rs.errorMessage = "";
        rs.scrapeDateTime = DateTime.UtcNow;
        await DbCtx.SaveChangesAsync();
        return rs;
    }


    public async Task UpdateLinkCountOverLimit(pageTbl page, int linkCount, int linkCountOverLimit)
    {
        await using var db = new SqlConnection(DbConnString);
        await db.ExecuteAsync("pageUpdateLinkCountOverLimit", new { pageId = page.id, linkCount, linkCountOverLimit });
    }

    public async Task UpdateLinkCount(pageTbl page, int linkCount)
    {
        await using var db = new SqlConnection(DbConnString);
        await db.ExecuteAsync("pageUpdateLinkCount", new { pageId = page.id, linkCount });
    }

    public async Task LinksDeleteForPage(int pageId)
    {
        TimeoutRetry.Reset();
        while (TimeoutRetry.Running)
        {
            try
            {
                var timeTaken = new TimeTaken();
                await using var db = new SqlConnection(DbConnString);
                await db.ExecuteAsync("linkDeleteForPageSp", new { pageId });

                Console.WriteLine($"LinksDeleteForPage: links for {pageId} deleted in {timeTaken.Seconds()} seconds");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine(ex.Message);
                Console.WriteLine("----------------------------------------------------");
                if (TimeoutRetry.RetriesMaxedOut)
                {
                    throw;
                }

                TimeoutRetry.Delay();
            }
        }
    }

    public pageTbl? PageLookup(scrapeTbl scrape)
    {
        return DbCtx.pageTbl
            .AsNoTracking()
            .SingleOrDefault(x => x.cleanLink == scrape.cleanLink);
    }




    public async Task LinkAdd(pageTbl page, linkTbl link)
    {
        await DbCtx.Procedures.linkAddSpAsync(page.id,
            link.indexOnPage,
            link.host,
            link.cleanLink,
            link.fullLink,
            link.rawLink,
            link.innerHtml,
            link.outerHtml,
            link.errorMessage);
        if (!link.cleanLink.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        await ScrapeAdd(link.host, link.cleanLink);
    }

    public async Task ScrapeAdd(string host, string cleanLink)
    {
        await HostAdd(host);
        if (ShouldSkip(cleanLink))
        {
            Console.WriteLine($"Skipping link: {cleanLink}");
            return;
        }
        if (!cleanLink.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        await DbCtx.Procedures.scrapeAddSpAsync(host, cleanLink);
    }   

    public class ScrapeQueueModel
    {
        public scrapeQueueQry QueueItem { get; set; } = null!;
        public int QueueCount { get; set; }
    }

    #region Host Methods

    /// <summary>
    /// Use internally, will set the maxPageToScrape to -1 and category to empty string.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public async Task<hostTbl> HostAdd(string host)
    {
        return await HostAdd(new Uri($"https://{host}"), -1, "");
    }
    public async Task<hostTbl> HostAdd(Uri uri, int maxPageToScrape, string category)
    {
        var tmp = await HttpClientHelper.GetHttpClientResponse(uri);

        if (tmp.StatusCode == 0)
        {
            return await HostAddError(tmp, maxPageToScrape, category);
        }

        if (!tmp.IsRedirected)
        {
            return await HostAddNonRedirect(maxPageToScrape, category, tmp.HttpClientResponse);
        }

        await HostAddRedirect(maxPageToScrape, category, tmp.HttpClientResponse);
        return await HostAddNonRedirect(maxPageToScrape, category, tmp.HttpClientResponse);
    }

    private async Task<hostTbl> HostAddError(HttpClientResponseContainer tmp, int maxPageToScrape, string category)
    {
        var rs = DbCtx.hostTbl.SingleOrDefault(x => x.host == tmp.RequestUri.Host);
        if (rs != null)
        {
            rs.errorMessage = Jeff32819DLL.MiscCore20.Code.Truncate(tmp.ErrorMessage, 1000);
            rs.redirectStatusCode = 0;
            rs.redirectedToHost = "";
            rs.redirectLastVerified = null;
            await DbCtx.SaveChangesAsync();
            return rs;
        }

        var newRs = DbCtx.hostTbl.Add(new hostTbl
        {
            errorMessage = Jeff32819DLL.MiscCore20.Code.Truncate(tmp.ErrorMessage, 1000),
            maxPageToScrape = maxPageToScrape,
            category = category,
            host = tmp.RequestUri.Host,
            redirectedToHost = "",
            redirectStatusCode = 0,
            redirectLastVerified = null
        });
        await DbCtx.SaveChangesAsync();
        return newRs.Entity;
    }

    private async Task<hostTbl> HostAddRedirect(int maxPageToScrape, string category, HttpClientResponse response)
    {
        var redirectedHost = response.RedirectedFrom!.FromUrl.Host;
        var rs = DbCtx.hostTbl.SingleOrDefault(x => x.host == redirectedHost);
        if (rs != null)
        {
            rs.redirectedToHost = response.RequestUri.Host;
            rs.redirectStatusCode = response.RedirectedFrom!.StatusCode;
            rs.redirectLastVerified = DateTime.UtcNow;
            await DbCtx.SaveChangesAsync();


            if (!string.IsNullOrEmpty(rs.category) || string.IsNullOrEmpty(category))
            {
                return rs; // DO NOT OVERWRITE CATEGORY IF ALREADY SET
            }

            rs.category = category;
            await DbCtx.SaveChangesAsync();
            return rs;
        }

        var newRs = DbCtx.hostTbl.Add(new hostTbl
        {
            maxPageToScrape = maxPageToScrape,
            category = category,
            host = redirectedHost,
            redirectedToHost = response.RequestUri.Host,
            redirectStatusCode = response.RedirectedFrom!.StatusCode,
            redirectLastVerified = DateTime.UtcNow
        });
        await DbCtx.SaveChangesAsync();
        return newRs.Entity;
    }


    private async Task<hostTbl> HostAddNonRedirect(int maxPageToScrape, string category, HttpClientResponse response)
    {
        var rs = DbCtx.hostTbl.SingleOrDefault(x => x.host == response.RequestUri.Host);
        if (rs != null)
        {
            if (!string.IsNullOrEmpty(rs.redirectedToHost))
            {
                rs.redirectedToHost = null;
                rs.redirectStatusCode = 0;
                rs.redirectLastVerified = null;
                await DbCtx.SaveChangesAsync();
                ;
            }

            if (!string.IsNullOrEmpty(rs.category) || string.IsNullOrEmpty(category))
            {
                return rs; // DO NOT OVERWRITE CATEGORY IF ALREADY SET
            }

            rs.category = category;
            await DbCtx.SaveChangesAsync();
            return rs;
        }

        var newRs = DbCtx.hostTbl.Add(new hostTbl
        {
            maxPageToScrape = maxPageToScrape,
            category = category,
            host = response.RequestUri.Host
        });
        await DbCtx.SaveChangesAsync();
        return newRs.Entity;
    }

    #endregion

    #region Database Reset Methods

    public void DbReset()
    {
        Console.WriteLine("Press 'C' to clear the database or any key to continue...");
        var key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.C)
        {
            DbResetWithoutWarning();
        }
        else
        {
            Console.WriteLine("Continuing without clearing the database.");
        }
    }


    public void DbResetWithoutWarning()
    {
        Console.WriteLine("Clearing database...");
        DbCtx.Database.ExecuteSqlRaw("DELETE FROM [linkTbl]");
        DbCtx.Database.ExecuteSqlRaw("DELETE FROM [pageTbl]");
        DbCtx.Database.ExecuteSqlRaw("DELETE FROM [scrapeTbl]");
        DbCtx.Database.ExecuteSqlRaw("DELETE FROM [hostTbl]");
        Console.WriteLine("Database cleared.");
        HostManager.Reset();

    }

    #endregion

    /// <summary>
    /// Should skip if items like .exe or .pdf are in the link.
    /// </summary>
    /// <param name="cleanLink"></param>
    /// <returns></returns>
    public bool ShouldSkip(string cleanLink) => Regex.IsMatch(cleanLink, @"\.(exe|pdf)", RegexOptions.IgnoreCase);

    public async Task PageAdd(string host, string fullLink, string cleanLink)
    {
        if(ShouldSkip(cleanLink))
        {
            Console.WriteLine($"Skipping link: {cleanLink}");
            return;
        }
        await DbCtx.Procedures.pageAddSpAsync(host, fullLink, cleanLink);
        await ScrapeAdd(host, cleanLink);
    }

    public List<hostPageLinkErrorsQry> ReportPagesForDomain(Uri uri)
    {
        return DbCtx.hostPageLinkErrorsQry.Where(x => x.host == uri.Host)
            .OrderBy(x => x.scrapeCleanLink)
            .ThenBy(x => x.pageCleanLink)
            .ToList();
    }
    public List<hostTbl> GetReportRs()
    {
        return DbCtx.hostTbl.Where(x => x.maxPageToScrape >= 0 && !x.reportDone).ToList();
    }


    #region Seed

    public async Task SeedAdd(string rawUrl, string category)
    {
        var uri = new UriSections(rawUrl, true);
        if (!uri.IsValid)
        {
            throw new Exception("Invalid URL");
        }

        await HostManager.Add(uri.Uri.Host, category, 100);
        await DbCtx.Procedures.pageAddSpAsync(uri.Uri.Host, uri.SchemeHost, uri.SchemeHost);
        await ScrapeAdd(uri.Uri.Host, uri.Uri.OriginalString);
    }

    #endregion

    public async Task PageLinkCountUpdate(pageTbl page, int linkCount)
    {
        await using var db = new SqlConnection(DbConnString);
        await db.ExecuteAsync("update pageTbl set linkCount = @linkCount where id = @pageId", new { pageId = page.id, linkCount });
    }


};