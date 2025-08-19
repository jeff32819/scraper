using CodeBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ScraperCode.DbCtx;

namespace ScraperCode
{
    public static class HostBuilderFactory
    {
        public static HostBuilderSetup Create()
        {
            const string dbConnString1 = "server=.\\dev14;database=WebScraper;trusted_connection=true;TrustServerCertificate=True";
            const string dbConnString2 = "server=.\\dev14;database=Scraper02;trusted_connection=true;TrustServerCertificate=True";

    
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
            var tmp = new HostBuilderSetup
            {
                DbConnString1 = dbConnString1,
                DbConnString2 = dbConnString2,
                DbService1 = hostService.Services.GetRequiredService<DbService>(),
                DbService2 = hostService.Services.GetRequiredService<DbService02>()
            };
            var filePaths = new LogFilePath(@"t:\ScraperApp2");
            var logger = new NLogSetup(true);
            logger.SetFileTarget(filePaths.Log);
            // logger.SetDbTarget(dbConnString2);
            tmp.Logger = logger.GetLogger();
            return tmp;
        }
    }

    public class HostBuilderSetup
    {
        public string DbConnString1 { get; set; } = "";
        public string DbConnString2 { get; set; } = "";
        public DbService DbService1 { get; set; } = null!;
        public DbService02 DbService2 { get; set; } = null!;
        public NLogger Logger { get; set; } = null!;
    }
}
