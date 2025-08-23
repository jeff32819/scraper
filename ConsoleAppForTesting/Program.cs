using ScraperCode;
using ScraperCode.Models;

var setup = HostBuilderFactory.Create();

// await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://business.lakenonacc.org");
// var result1 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://sebastianmoving.net");
// var result2 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://www.sebastianmoving.net");
// var result1 = await ConsoleAppForTesting.Test.GetFromWeb(setup.DbSvc01, "https://jeff32819.com");


var url = "https://kettlecreeksnacks.com";




setup.DbSvc02.DbReset();


await setup.DbSvc02.SeedAdd("https://kettlecreeksnacks.com", "test");
Console.WriteLine("START -- SCRAPING");
await Scraper02.Process(setup.DbSvc02, setup.Logger);
Console.WriteLine();
Console.WriteLine("DONE -- SCRAPING");
Console.WriteLine();