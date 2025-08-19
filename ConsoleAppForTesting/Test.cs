using ScraperCode;
using ScraperCode.Models;

namespace ConsoleAppForTesting;

public static class Test
{
    public static async Task<IScrapeRequest> GetFromWeb(DbService dbSvc, string url)
    {
        var tmp = await HttpClientHelper.GetAsync(new Uri(url));
        return tmp;
    }
}