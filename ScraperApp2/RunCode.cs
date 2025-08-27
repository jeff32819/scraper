using System.Text.RegularExpressions;
using CodeBase;
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

    public class FileRow
    {
        public string Category { get; set; }
        public string Link { get; set; }
    }

    public static bool IsDevServer(string devPcName)
    {
        return (Environment.MachineName.Equals(devPcName, StringComparison.OrdinalIgnoreCase));
    }

}