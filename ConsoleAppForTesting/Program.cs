using System.Reflection;
using CodeBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ScraperCode;
using ScraperCode.DbCtx;

const string dbConnString1 = "server=.\\dev14;database=WebScraper;trusted_connection=true;TrustServerCertificate=True";
const string dbConnString2 = "server=.\\dev14;database=Scraper02;trusted_connection=true;TrustServerCertificate=True";
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
            opt.UseSqlServer(dbConnString1);
            //opt.LogTo(Console.WriteLine, LogLevel.Warning);
            opt.LogTo(_ => { }); // Uncomment to disable all EF Core logging
        });
        services.AddTransient<DbService>();
        services.AddDbContext<WebScraperContext>(opt =>
        {
            opt.UseSqlServer(dbConnString2);
            //opt.LogTo(Console.WriteLine, LogLevel.Warning);
            opt.LogTo(_ => { }); // Uncomment to disable all EF Core logging
        });
        services.AddTransient<DbService02>();
    })
    .Build();
var dbSvc = hostService.Services.GetRequiredService<DbService>();
var filePaths = new LogFilePath(@"t:\ScraperApp2");
var logger = new NLogSetup(true);
logger.SetFileTarget(filePaths.Log);
//logger.SetDbTarget(dbConnString2);
var log = logger.GetLogger();


// await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://business.lakenonacc.org");
//var result1 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://sebastianmoving.net");
//var result2 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://www.sebastianmoving.net");

Console.WriteLine();