using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScraperApp2;
using ScraperCode;
using ScraperCode.DbCtx;


const string dbConnString = "server=.\\dev14;database=WebScraper;trusted_connection=true;TrustServerCertificate=True";
var appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

var hostService = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders(); // Optional: removes default providers
        //logging.AddConsole();
        //logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning); // Only show warnings/errors from EF Core
    })
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<WebScraperContext>(opt =>
        {
            opt.UseSqlServer(dbConnString);
            //opt.LogTo(Console.WriteLine, LogLevel.Warning);
            opt.LogTo(_ => { }); // Uncomment to disable all EF Core logging
        });
        services.AddTransient<DbService>();
    })
    .Build();
var dbSvc = hostService.Services.GetRequiredService<DbService>();
var filePaths = new LogFilePath(@"t:\ScraperApp2");
var logger = new NLogSetup(true);
logger.SetFileTarget(filePaths.Log);
logger.SetDbTarget(dbConnString);
var log = logger.GetLogger();

Console.Title = $"ScraperApp2 (v{appVersion})";

var devServer = "panamacity";
if (RunCode.IsDevServer(devServer))
{
    Console.WriteLine($"DEV SERVER: {devServer}");
    Console.WriteLine("Press 'C' to clear the database or any key to continue...");
    var key = Console.ReadKey(true);
    if(key.Key == ConsoleKey.C)
    {
        Console.WriteLine("Clearing database...");
        dbSvc.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[linkTbl]");
        dbSvc.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[pageTbl]");
        dbSvc.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[scrapeTbl]");
        dbSvc.DbCtx.Database.ExecuteSqlRaw("DELETE FROM [WebScraper].[dbo].[hostTbl]");
        Console.WriteLine("Database cleared.");
    }
    else
    {
        Console.WriteLine("Continuing without clearing the database.");
    }
    RunCode.AddManual(dbSvc, "https://jeff32819.com");
}
else
{
    await RunCode.FromFile(dbSvc, filePaths, log);
}




var scraper = new Scraper(dbSvc, log);
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
