using System.Reflection;
using CodeBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScraperApp2;
using ScraperCode;
using ScraperCode.DbCtx;



var appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

var setup = ScraperCode.HostBuilderFactory.Create();

Console.Title = $"ScraperApp2 (v{appVersion})";

const string devServer = "panamacity";
if (RunCode.IsDevServer(devServer))
{
    Console.WriteLine($"DEV SERVER: {devServer}");
    Console.WriteLine("Press 'C' to clear the database or any key to continue...");
    var key = Console.ReadKey(true);
    if(key.Key == ConsoleKey.C)
    {
        Console.WriteLine("Clearing database...");
        setup.DbSvc01.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[linkTbl]");
        setup.DbSvc01.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[pageTbl]");
        setup.DbSvc01.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[scrapeTbl]");
        setup.DbSvc01.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[hostTbl]");
        Console.WriteLine("Database cleared.");
    }
    else
    {
        Console.WriteLine("Continuing without clearing the database.");
    }
    RunCode.AddManual(setup.DbSvc01, "https://jeff32819.com");
}
else // NOT DEV SERVER
{
    await RunCode.FromFile(setup.DbSvc01, filePaths, log);
}




var scraper = new Scraper(setup.DbSvc01, log);
await scraper.ProcessScrapeHtml();




var reportRs = dbSvc.GetReportRs();
foreach (var item in reportRs)
{
    await ScrapeReport.Process(dbSvc, $"https://{item.host}");
    item.reportDone = true;
    dbSvc.DbCtx.hostTbl.Update(item);
    dbSvc.DbCtx.SaveChanges();
    Console.WriteLine($"Report done for {item.host}");
}

Console.WriteLine("");
Console.WriteLine("DONE!!!       Press any key to exit");
Console.WriteLine("");
Console.ReadKey();
