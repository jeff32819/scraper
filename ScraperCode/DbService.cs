using System.Text.RegularExpressions;
using DbWebScraper.Models;
using Jeff32819DLL.MiscCore20;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScraperCode.Models;

namespace ScraperCode;

public class DbService
{
    public DbService(WebScraperContext db)
    {
        DbCtx = db;
        DbConnString = DbCtx.Database.GetConnectionString() ?? throw new Exception("No database connection string");
    }

    public string DbConnString { get; }
    public WebScraperContext DbCtx { get; }

    public TimeoutRetry TimeoutRetry => new(3, 10000, "t:\\timeout-log.txt");

    public List<hostWithoutAnyPages> HostsWithoutPages()
    {
        return DbCtx.hostWithoutAnyPages.ToList();
    }

    public int GetPageCount()
    {
        return DbCtx.pageTbl.Count();
    }

    public int HostPageCount(int hostId)
    {
        return DbCtx.pageTbl.Count(x => x.hostId == hostId);
    }

    public List<scapeLinksThatShouldBeInPages> ScapeLinksThatShouldBeInPages()
    {
        return DbCtx.scapeLinksThatShouldBeInPages.ToList();
    }


    public List<hostTbl> GetReportRs()
    {
        return DbCtx.hostTbl.Where(x => x.maxPageToScrape >= 0 && !x.reportDone).ToList();
    }

    public hostTbl HostAddIfNotExists(Uri uri)
    {
        return HostAdd(uri, -1, "");
    }


    public void HostRedirectionUpdate(string oldHost, string newHost)
    {
        var oldUri = new Uri(oldHost);
        var newUri = new Uri(newHost);

        var rs = DbCtx.hostTbl.SingleOrDefault(x => x.host == oldUri.Host);
        if(rs == null)
        {
            throw new Exception($"Host not found in database: {oldHost}"); // should not happen
        }

        if (!string.IsNullOrEmpty(rs.redirectedToUrl))
        {
            return;
        }
        rs.redirectedToUrl = newUri.Host;
        DbCtx.SaveChanges();
    }


    public hostTbl HostAddSeed(Uri uri, int maxPageToScrape, string category)
    {
        return HostAdd(uri, maxPageToScrape, category);
    }

    private hostTbl HostAdd(Uri uri, int maxPageToScrape, string category)
    {
        var rs = DbCtx.hostTbl.SingleOrDefault(x => x.host == uri.Host);
        if (rs != null)
        {
            if (!string.IsNullOrEmpty(rs.category) || string.IsNullOrEmpty(category))
            {
                return rs;
            }

            rs.category = category;
            DbCtx.SaveChanges();
            return rs;
        }

        var newRs = new hostTbl
        {
            maxPageToScrape = maxPageToScrape,
            host = uri.Host,
            category = category,
            path = "" // adding later // uri.AbsolutePath
        };
        DbCtx.hostTbl.Add(newRs);
        DbCtx.SaveChanges();
        return newRs;
    }

    public hostTbl HostGet(Uri uri)
    {
        var rs = DbCtx.hostTbl.SingleOrDefault(x => x.host == uri.Host);
        if (rs != null)
        {
            return rs;
        }

        throw new Exception($"Host not found in database: {uri.Host}");
    }


    public scrapeTbl ScrapeAdd(scrapeTbl scrape)
    {
        var rs = DbCtx.scrapeTbl.SingleOrDefault(x => x.absoluteUri == scrape.absoluteUri);
        if (rs != null)
        {
            return rs;
        }

        DbCtx.scrapeTbl.Add(scrape);
        DbCtx.SaveChanges();
        return scrape;
    }

    public void FileTypeToScrapeAdd(string txt)
    {
        var rs = DbCtx.fileTypeAllowedToScrapeTbl.SingleOrDefault(x => x.fileExt == txt);
        if (rs != null)
        {
            return;
        }

        DbCtx.fileTypeAllowedToScrapeTbl.Add(new fileTypeAllowedToScrapeTbl
        {
            fileExt = txt
        });
        DbCtx.SaveChanges();
    }


    //public List<linkTbl> ReportLinkErrorForPage(int pageId)
    //{
    //    return DbCtx.linkTbl.Where(x => x.pageId == pageId).OrderBy(x => x.linkAbsolute && x.).ToList();
    //}


    //public scrapeTbl ScrapeUpdateHeadSuccess(int id, IScrapeRequest response)
    //{
    //    var rs = DbCtx.scrapeTbl.Single(x => x.id == id);
    //    rs.headStatusCode = response.StatusCode;
    //    rs.headContentType = response.ContentType;
    //    rs.headResponseHeaders = JsonConvert.SerializeObject(response.ResponseHeaders, Formatting.None);
    //    rs.headContentHeaders = JsonConvert.SerializeObject(response.ContentHeaders, Formatting.None);
    //    rs.statusCode = response.StatusCode;
    //    DbCtx.scrapeTbl.Update(rs);
    //    DbCtx.SaveChanges();
    //    return rs;
    //}

    public void ScrapeUpdateSuccess(int id, IScrapeRequest response)
    {
        var rs = DbCtx.scrapeTbl.Single(x => x.id == id);
        rs.headStatusCode = response.StatusCode;
        rs.headContentType = response.ContentType;
        rs.headResponseHeaders = JsonConvert.SerializeObject(response.ResponseHeaders, Formatting.None);
        rs.headContentHeaders = JsonConvert.SerializeObject(response.ContentHeaders, Formatting.None);
        rs.statusCode = response.StatusCode;
        rs.html = response.Html;
        rs.scrapeDateTime = DateTime.UtcNow;
        DbCtx.scrapeTbl.Update(rs);
        DbCtx.SaveChanges();
    }

    public scrapeTbl ScrapeUpdateHeadFail(int id, Exception ex)
    {
        var rs = DbCtx.scrapeTbl.Single(x => x.id == id);
        rs.headStatusCode = "ERROR";
        rs.headErrorMessage = ex.Message;
        DbCtx.scrapeTbl.Update(rs);
        DbCtx.SaveChanges();
        return rs;
    }


    public void ScrapeUpdateHtmlFail(int id, Exception ex)
    {
        var rs = DbCtx.scrapeTbl.Single(x => x.id == id);
        rs.statusCode = "ERROR";
        rs.html = ex.Message;
        rs.scrapeDateTime = DateTime.UtcNow;
        DbCtx.scrapeTbl.Update(rs);
        DbCtx.SaveChanges();
    }

    //public int PageGetId(string absoluteUri)
    //{
    //    var pageRs = DbCtx.pageTbl.SingleOrDefault(x => x.absoluteUri == absoluteUri);
    //    if (pageRs == null)
    //    {
    //        throw new Exception($"Page not found in database: {absoluteUri}");
    //    }
    //}

    public void PageAdd(pageTbl pg)
    {
        if (DbCtx.pageTbl.Any(x => x.absoluteUri == pg.absoluteUri))
        {
            return;
        }

        DbCtx.pageTbl.Add(pg);
        DbCtx.SaveChanges();
    }

    //public pageTbl? PageLookup(string absoluteUri)
    //{
    //    var tmp = Code.TrimLastSlash(absoluteUri);
    //    return DbCtx.pageTbl.SingleOrDefault(x => x.absoluteUri == tmp);
    //}



    /// <summary>
    ///     Links do not need to be checked before being added, as
    ///     they are always deleted before being added for a page.
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public void LinkAdd(linkTbl link)
    {
        if (!Regex.IsMatch(link.absoluteUri, "^http", RegexOptions.IgnoreCase))
        {
            // jeff2do need to later log tel & email
            return;
            //throw new Exception("Link must start with http or https");
        }

        var tmpRs = DbCtx.linkTbl.Add(link);
        DbCtx.SaveChanges();
    }

    public ScrapeQueue ScrapeQueue()
    {
        TimeoutRetry.Reset();
        while (TimeoutRetry.Running)
        {
            try
            {
                var count = DbCtx.scrapeQueueQry.Count();
                if (count == 0)
                {
                    return new ScrapeQueue
                    {
                        QueueCount = 0,
                        QueueItem = new ScrapeQueue.QueueItemModel()
                    };
                }

                var skip = new Random().Next(count);
                var rs = DbCtx.scrapeQueueQry.Skip(skip).Take(1).First();
                return new ScrapeQueue
                {
                    QueueCount = count,
                    QueueItem = new ScrapeQueue.QueueItemModel(rs)
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

    public void PageUpdateLinkCount(int pageId, int linkCount)
    {
        TimeoutRetry.Reset();
        while (TimeoutRetry.Running)
        {
            try
            {
                var timeTaken = new TimeTaken();
                using var db = new SqlConnection(DbConnString);
                const string sql = "UPDATE pageTbl SET linkCount = @linkCount WHERE id = @pageId";
                var cmd = new SqlCommand(sql, db);
                cmd.Connection.Open();
                var param1 = cmd.CreateParameter();
                param1.ParameterName = "@linkCount";
                param1.Value = linkCount;
                cmd.Parameters.Add(param1);
                var param2 = cmd.CreateParameter();
                param2.ParameterName = "@pageId";
                param2.Value = pageId;
                cmd.Parameters.Add(param2);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                db.Close();
                Console.WriteLine($"PageUpdateLinkCount: {pageId} updated with {linkCount} links in {timeTaken.Seconds()} seconds");
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

    public void LinksDeleteForPage(int pageId)
    {
        TimeoutRetry.Reset();
        while (TimeoutRetry.Running)
        {
            try
            {
                var timeTaken = new TimeTaken();
                using var db = new SqlConnection(DbConnString);
                const string sql = "DELETE FROM linkTbl WHERE pageId = @pageId";
                var cmd = new SqlCommand(sql, db);
                cmd.Connection.Open();
                var param = cmd.CreateParameter();
                param.ParameterName = "@pageId";
                param.Value = pageId;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                db.Close();
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
}