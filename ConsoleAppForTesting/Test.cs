using ScraperCode;
using ScraperCode.Models;

namespace ConsoleAppForTesting;

public static class Test
{
    public static async Task<IScrapeRequest> GetFromWeb(DbService dbSvc, string url)
    {
        var tmp = await HttpClientHelper.GetAsync(new Uri(url));

        if (tmp.WasRedirected)
        {

            dbSvc.HostAddSeed(new Uri(tmp.Url), 100, "");
        }


        return tmp;
    }
}