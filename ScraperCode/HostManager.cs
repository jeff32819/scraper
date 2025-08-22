using System.Diagnostics;
using DbScraper02.Models;
// ReSharper disable MemberCanBePrivate.Global

namespace ScraperCode
{
    public class HostManager
    {
        public HostManager(Scraper02Context db)
        {
            Db = db;
            Reset();
        }

        public void Reset()
        {
            Items.Clear();
            foreach (var item in Db.hostTbl.ToList())
            {
                Items.Add(item.host, item);
            }
        }

        private Scraper02Context Db { get; }

        public async Task<hostTbl> Lookup(string host) => await Add(host, "");

        public async Task<hostTbl> Add(string host, string category, int maxPagesToScrape = -1)
        {
            if (Items.TryGetValue(host, out var item))
            {
                return item;
            }
            var rs = new hostTbl
            {
                host = host,
                maxPageToScrape = maxPagesToScrape,
                addedDateTime = DateTime.UtcNow
            };

            try
            {
                Db.hostTbl.Add(rs);
                await Db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw;
            }


            Items.Add(host, rs);
            return rs;
        }
        private Dictionary<string, hostTbl> Items { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
