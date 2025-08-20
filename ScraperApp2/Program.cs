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
    await setup.DbSvc02.SeedAdd("https://brittanymooreproduction.com");

}
else // NOT DEV SERVER
{
    await RunCode.FromFile(setup.DbSvc01, filePaths, setup.Logger);
}


//var rs = setup.DbSvc02.DbCtx.tmpHostTransferQry.ToList();
//foreach (var host in rs)
//{
//    Console.WriteLine($"Adding host: {host.host}");
//    await setup.DbSvc02.HostAdd(new Uri($"https://{host.host}"), host.maxPageToScrape, host.category);
//}


await Scraper02.Process(setup.DbSvc02, setup.Logger);


//var reportRs = setup.DbSvc01.GetReportRs();
//foreach (var item in reportRs)
//{
//    await ScrapeReport.Process(setup.DbSvc01, $"https://{item.host}");
//    item.reportDone = true;
//    setup.DbSvc01.DbCtx.hostTbl.Update(item);
//    setup.DbSvc01.DbCtx.SaveChanges();
//    Console.WriteLine($"Report done for {item.host}");
//}

Console.WriteLine("");
Console.WriteLine("DONE!!!       Press any key to exit");
Console.WriteLine("");
Console.ReadKey();