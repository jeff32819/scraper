using CodeBase;
using DbScraper02.Models;
using DbWebScraper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace ScraperCode;

public static class HostBuilderFactory
{
    public static HostBuilderSetup Create()
    {
        const string dbConnString01 = "server=.\\dev14;database=WebScraper;trusted_connection=true;TrustServerCertificate=True";
        const string dbConnString02 = "server=.\\dev14;database=Scraper02;trusted_connection=true;TrustServerCertificate=True";


        var hostService = Host.CreateDefaultBuilder() // could also pass args here from command line
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
                    opt.UseSqlServer(dbConnString01);
                    //opt.LogTo(Console.WriteLine, LogLevel.Warning);
                    opt.LogTo(_ => { }); // Uncomment to disable all EF Core logging
                });
                services.AddTransient<DbService>();
                services.AddDbContext<Scraper02Context>(opt =>
                {
                    opt.UseSqlServer(dbConnString02);
                    //opt.LogTo(Console.WriteLine, LogLevel.Warning);
                    opt.LogTo(_ => { }); // Uncomment to disable all EF Core logging
                });
                services.AddTransient<DbService02>();
            })
            .Build();
        var tmp = new HostBuilderSetup
        {
            DbConnString01 = dbConnString01,
            DbConnString02 = dbConnString02,
            DbSvc01 = hostService.Services.GetRequiredService<DbService>(),
            DbSvc02 = hostService.Services.GetRequiredService<DbService02>()
        };
        var filePaths = new LogFilePath(@"t:\ScraperApp2");
        var logger = new NLogSetup(true);
        logger.SetFileTarget(filePaths.Log);
        logger.SetDbTarget(dbConnString02);
        tmp.Logger = logger.GetLogger();
        return tmp;
    }
}

public class HostBuilderSetup
{
    public string DbConnString01 { get; set; } = "";

    public DbService DbSvc01 { get; set; } = null!;
    public string DbConnString02 { get; set; } = "";
    public DbService02 DbSvc02 { get; set; } = null!;
    public NLogger Logger { get; set; } = null!;
}