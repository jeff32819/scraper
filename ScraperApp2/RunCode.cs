using System.Text.RegularExpressions;
using CodeBase;
using DbScraper02.Models;
using Jeff32819DLL.MiscCore20;
using ScraperCode;

namespace ScraperApp2;

public static class RunCode
{
    /// <summary>
    ///     Get list of files to process from a file.
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>> FromFileToList(string filePath, NLogger log, bool domainOnly = true)
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


    public static async Task<List<FileRow>> FromFile(LogFilePath filePaths, NLogger log)
    {
        var arr = new List<FileRow>();
        const int maxPageToScrape = 100;


        log.Info("RunCode.FromFile... STARTING");


        Directory.CreateDirectory(filePaths.LinkFileFolder);
        var files = Directory.GetFiles(filePaths.LinkFileFolder);
        if (files.Length == 0)
        {
            log.Info("No files found in links-to-parse folder");
            return arr;
        }

        foreach (var file in files)
        {
            var seeds = await FromFileToList(file, log);
            arr.AddRange(seeds.Select(seed => new FileRow { Link = seed, Category = Path.GetFileNameWithoutExtension(file) }));
            log.Info($"Page seeds count = [{seeds.Count}]");
        }

        log.Info("RunCode.FromFile... DONE");
        return arr;
    }

    public static bool IsDevServer(string devPcName)
    {
        return Environment.MachineName.Equals(devPcName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Run report by host e.g. jeff32819.com
    /// </summary>
    /// <param name="db"></param>
    /// <param name="host"></param>
    /// <param name="saveChanges"></param>
    /// <returns></returns>
    public static async Task ReportRunByHost(DbService02 db, string host, bool saveChanges = true)
    {
        var rs = db.HostByHostName(host);
        await ReportRunEach(db, rs, saveChanges);
    }

    public static async Task RunReports(DbService02 db)
    {
        var reportRs = db.GetReportRs();
        foreach (var item in reportRs)
        {
            await ReportRunEach(db, item, false);
        }
    }

    private static async Task ReportCreateAndSave(DbService02 db, hostTbl item)
    {
        const string rootFolder = "t:\\scraper-bad-link-reports";
        Directory.CreateDirectory(rootFolder);
        Directory.CreateDirectory(Path.Combine(rootFolder, "done"));
        var filePath = Path.Combine(rootFolder, $"{item.host}.html");
        var donePath = Path.Combine(rootFolder, "done", $"{item.host}.html");
        if (File.Exists(donePath))
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath); // file should not be here if done file exists.
            return;
        }

        var reportText = await ScrapeReport.ProcessRazor(db, $"https://{item.host}");
        await File.WriteAllTextAsync(filePath, reportText);
    }

    private static async Task ReportRunEach(DbService02 db, hostTbl item, bool saveChanges = true)
    {
        await ReportCreateAndSave(db, item);

        // not going to update db for now, just use file system with root & done folder to track.
        //if (!saveChanges)
        //{
        //    return;
        //}
        // await ReportMarkDone(db, item);
    }

    private static async Task ReportMarkDone(DbService02 db, hostTbl item)
    {
        item.reportDone = true;
        db.DbCtx.hostTbl.Update(item);
        await db.DbCtx.SaveChangesAsync();
    }

    public class FileRow
    {
        public string Category { get; set; } = "";
        public string Link { get; set; } = "";
    }
}