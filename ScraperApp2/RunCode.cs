using System.Text.RegularExpressions;
using CodeBase;
using ScraperCode;

namespace ScraperApp2;

public static class RunCode
{
    /// <summary>
    ///     Get list of files to process from a file.
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>> FromFileToList(DbService dbSvc, string filePath, NLogger log, bool domainOnly = true)
    {
        var arr = new List<string>();
        if (!File.Exists(filePath))
        {
            Console.WriteLine(filePath);
            await File.WriteAllTextAsync(filePath, "");
            return arr; // empty or non-existent file.
        }

        await foreach (var url in File.ReadLinesAsync(filePath))
        {
            log.Info($"line: {url}");
            if (string.IsNullOrEmpty(url))
            {
                log.Info("Empty line");
                continue;
            }

            if (url.StartsWith('*'))
            {
                log.Info($"returning {arr.Count} items");
                log.Info("Found line with '*'... EXITING");
                return arr;
            }

            var tmp = CalcLink(url, domainOnly);
            if (string.IsNullOrEmpty(tmp))
            {
                log.Info($"Skipping empty link: {tmp}");
                continue;
            }

            arr.Add(tmp);
            log.Info($"adding [{tmp}] count = [{arr.Count}]");
        }

        log.Info($"returning {arr.Count} items");
        return arr;
    }


    private static string CalcLink(string link, bool domainOnly)
    {
        link = link.Replace("\t", "");
        if (string.IsNullOrEmpty(link))
        {
            return "";
        }

        if (!Regex.IsMatch(link, "^http", RegexOptions.IgnoreCase))
        {
            link = $"https://{link}";
        }

        var uriSection = new UriSections(link, false);
        return uriSection.SchemeHost;
    }


    public static async Task FromFile(DbService dbSvc, LogFilePath filePaths, NLogger log)
    {
        const int maxPageToScrape = 100;


        log.Info("RunCode.FromFile... STARTING");

        var scraper = new Scraper(dbSvc, log);

        Directory.CreateDirectory(filePaths.LinkFileFolder);
        var files = Directory.GetFiles(filePaths.LinkFileFolder);
        if (files.Length == 0)
        {
            log.Info("No files found in links-to-parse folder");
            return;
        }
        foreach (var file in files)
        {
            var seeds = await FromFileToList(dbSvc, file, log);
            log.Info($"Page seeds count = [{seeds.Count}]");
            scraper.PageSeedsAsync(seeds, file, maxPageToScrape);
        }

        log.Info("RunCode.FromFile... DONE");
    }

    public static bool IsDevServer(string devPcName)
    {
        return (Environment.MachineName.Equals(devPcName, StringComparison.OrdinalIgnoreCase));
    }

}