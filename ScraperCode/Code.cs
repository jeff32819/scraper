using System.Diagnostics;

using AngleSharp;

using DbWebScraper.Models;

using ScraperCode.Models;

namespace ScraperCode;

public static class Code
{
    public static string CalcAbsoluteUri(UriSections uriSections)
    {
        return uriSections.SchemeHostPathQuery.TrimEnd('/');
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
                    throw new Exception("href is NULL for link tag");
                }

                var uri =  new UriSections(new Uri(absoluteUri), href);
                if (!uri.IsValid)
                {
                    throw new Exception("href is NOT VALID: " + href);
                }
                rv.Links.Add(new linkTbl
                {
                    scrapeId = -1, // scrape is not set here, it is set later
                    pageId = pageId,
                    indexOnPage = linkIndexOnPage++,
                    rawLink = href,
                    absoluteUri = uri.Uri.AbsoluteUri,
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