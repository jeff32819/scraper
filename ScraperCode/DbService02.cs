using System;

using DbScraper02.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ScraperCode.Models;

namespace ScraperCode
{
    public class DbService02
    {
        public DbService02(Scraper02Context db)
        {
            DbCtx = db;
            DbConnString = DbCtx.Database.GetConnectionString() ?? throw new Exception("No database connection string");
        }

        public string DbConnString { get; }
        public Scraper02Context DbCtx { get; }

        #region Host Methods
        public async Task<hostTbl> HostAdd(Uri uri, int maxPageToScrape, string category)
        {
            var tmp = await HttpClientHelper.GetHttpClientResponse(uri);

            if (!tmp.Success)
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
                maxPageToScrape = maxPageToScrape,
                category = category,
                host = tmp.RequestUri.Host,
                redirectedToHost = "",
                redirectStatusCode = 0,
                redirectLastVerified = null,
            });
            await DbCtx.SaveChangesAsync();
            return newRs.Entity;
        }

        private async Task<hostTbl> HostAddRedirect(int maxPageToScrape, string category, Models.HttpClientResponse response)
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


        private async Task<hostTbl> HostAddNonRedirect(int maxPageToScrape, string category, Models.HttpClientResponse response)
        {
            var rs = DbCtx.hostTbl.SingleOrDefault(x => x.host == response.RequestUri.Host);
            if (rs != null)
            {
                if(!string.IsNullOrEmpty(rs.redirectedToHost))
                {
                    rs.redirectedToHost = null;
                    rs.redirectStatusCode = 0;
                    rs.redirectLastVerified = null;
                    await DbCtx.SaveChangesAsync(); ;
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
            // DbCtx.Database.ExecuteSqlRaw("DELETE FROM [linkTbl]");
            // DbCtx.Database.ExecuteSqlRaw("DELETE FROM [pageTbl]");
            // DbCtx.Database.ExecuteSqlRaw("DELETE FROM [scrapeTbl]");
            DbCtx.Database.ExecuteSqlRaw("DELETE FROM [hostTbl]");
            Console.WriteLine("Database cleared.");
        }

        #endregion
    }
}
