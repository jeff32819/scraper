using System.Reflection;
using CodeBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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



