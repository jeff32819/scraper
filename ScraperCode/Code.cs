using System.Diagnostics;
using AngleSharp;
using Jeff32819DLL.HtmlParser;
using ScraperCode.DbCtx;
using ScraperCode.Models;

namespace ScraperCode;

public static class Code
{
    public static bool CalcIsInternalPage(DomainConfig parentDomain, string child)
    {
        var childDomain = new DomainConfig(child);
        Debug.Print("child  = " + childDomain.Domain);
        Debug.Print("parent = " + parentDomain.Domain);
        Debug.Print("");
        return string.Equals(childDomain.Domain, parentDomain.Domain, StringComparison.CurrentCultureIgnoreCase);
    }


    public static async Task<LinkParsed> GetLinksWithHtmlAsync(int pageId, string absoluteUri, string html)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        var rv = new LinkParsed();
        var linkIndexOnPage = 0;
        foreach (var a in document.QuerySelectorAll("a"))
        {
            try
            {
                var href = a.GetAttribute("href");
                if (href == null)
                {
                    throw new Exception("href is null for link tag");
                }

                var uri = UrlParser.GetUri(new Uri(absoluteUri), href);
                rv.Links.Add(new linkTbl
                {
                    scrapeId = -1, // scrape is not set here, it is set later
                    pageId = pageId,
                    indexOnPage = linkIndexOnPage++,
                    rawLink = href,
                    absoluteUri = uri.AbsoluteUri,
                    outerHtml = a.OuterHtml,
                    innerHtml = a.InnerHtml,
                    addedDateTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                rv.Errors.Add(a.OuterHtml);
                Debug.Print($"Error parsing link: {ex.Message}");
            }
        }

        return rv;
    }


    public static bool EscKeyPressed()
    {
        return Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape;
    }

    public static void DisplayDebugSql()
    {
        Debug.Print("/*** SQL START ***/");
        Debug.Print("/*");
        Debug.Print("DELETE FROM [WebScraper].[dbo].[scrapeTbl]");
        Debug.Print("DELETE FROM [WebScraper].[dbo].[linkTbl]");
        Debug.Print("DELETE FROM [WebScraper].[dbo].[pageTbl]");
        Debug.Print("*/");
        //Debug.Print($"-- DELETE FROM [WebScraper].[dbo].[pageTbl] WHERE host = '{StartUri.Host}'");
        //Debug.Print($"SELECT * FROM [WebScraper].[dbo].[pageTbl] WHERE host = '{StartUri.Host}'");
        Debug.Print("/*** FULL TABLES ***/");
        Debug.Print("SELECT * FROM [WebScraper].[dbo].[pageTbl]");
        Debug.Print("SELECT * FROM [WebScraper].[dbo].[scrapeTbl]");
        Debug.Print("SELECT * FROM [WebScraper].[dbo].[linkTbl]");
        Debug.Print("/*** SQL END ***/");
    }

    public static void Report(DbService dbSvc)
    {
        const int showHostWithLessThanXPages = 5;

        var hostRs = dbSvc.DbCtx.hostPageCountQry
            .Where(x => x.maxPageToScrape > 0 && x.pageCount <= showHostWithLessThanXPages)
            .OrderBy(x => x.host).ToList();

        var count = 0;

        using var writer = new StreamWriter("t:\\host-report.txt");
        foreach (var host in hostRs)
        {
            count++;

            writer.WriteLine($"{host.host} (#{count})");
            writer.WriteLine();
            writer.WriteLine($"\tmaxPage  : {host.maxPageToScrape}");
            writer.WriteLine($"\tpageCount: {host.pageCount}");

            var pages = dbSvc.DbCtx.pageScrapeQry
                .Where(x => x.host == host.host)
                .ToList();
            foreach (var page in pages)
            {
                writer.WriteLine($"\tPage: {page.pageAbsoluteUri}");
                var html = page.html ?? "";
                writer.WriteLine($"\t{page.statusCode} :: {html.Length}");
                writer.WriteLine();
                Console.WriteLine($"Page: {page.pageAbsoluteUri}");
            }

            writer.WriteLine();
        }

        writer.WriteLine();
    }
}