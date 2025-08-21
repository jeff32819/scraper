using System.Reflection;

using CodeBase;

using ScraperApp2;

using ScraperCode;

var appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
var filePaths = new LogFilePath(@"t:\ScraperApp2");
var setup = HostBuilderFactory.Create();
await setup.DbSvc02.StaticDataUpdate();

Console.Title = $"ScraperApp2 (v{appVersion})";

const string devServer = "panamacity";
if (RunCode.IsDevServer(devServer))
{
    Console.WriteLine($"DEV SERVER: {devServer}");
    //  add later after using db2 // setup.DbSvc02.DbReset();
    //await setup.DbSvc02.SeedAdd("https://jeremybuff.com", "import");

}
else // NOT DEV SERVER
{
 
}
var seeds = await RunCode.FromFile(filePaths, setup.Logger);
var count = 0;
foreach (var seed in seeds)
{
    count++;
    await setup.DbSvc02.SeedAdd(seed.Link, seed.Category);
}

if (count > 0)
{
    Console.WriteLine($"items added: {count}");
    Console.WriteLine("delete files now -- press any key to continue");
    Console.Read();
}

Console.WriteLine("START -- SCRAPING");
await Scraper02.Process(setup.DbSvc02, setup.Logger);
Console.WriteLine();
Console.WriteLine("DONE -- SCRAPING");
Console.WriteLine();
Console.WriteLine("Press R to run reports, or any other key to exit");
if (Console.ReadKey().Key != ConsoleKey.R)
{
    return;
}
var reportRs = setup.DbSvc02.GetReportRs();
foreach (var item in reportRs)
{
    await ScrapeReport.Process(setup.DbSvc02, $"https://{item.host}");
    item.reportDone = true;
    setup.DbSvc02.DbCtx.hostTbl.Update(item);
    setup.DbSvc02.DbCtx.SaveChanges();
    Console.WriteLine($"Report done for {item.host}");
}
