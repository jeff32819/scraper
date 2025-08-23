using ScraperCode;
using ScraperCode.Models;

var setup = HostBuilderFactory.Create();

// await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://business.lakenonacc.org");
// var result1 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://sebastianmoving.net");
// var result2 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://www.sebastianmoving.net");
// var result1 = await ConsoleAppForTesting.Test.GetFromWeb(setup.DbSvc01, "https://jeff32819.com");


var url = "https://kettlecreeksnacks.com";

var handler = new HttpClientHandler
{
    AllowAutoRedirect = false
};

var client = new HttpClient(handler);
client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/74.0.3729.1235");
client.Timeout = TimeSpan.FromSeconds(30); // Set timeout to 30 seconds
var tmp = await client.GetAsync(url);
var xxx = new HttpClientResponse(tmp, null);
Console.WriteLine("done1111111111111111111111");
Console.WriteLine(await xxx.GetContentAsync());
Console.WriteLine("done2222222222222222222222222");
Console.ReadKey();
return;


setup.DbSvc02.DbReset();


await setup.DbSvc02.SeedAdd("https://kettlecreeksnacks.com", "test");
Console.WriteLine("START -- SCRAPING");
await Scraper02.Process(setup.DbSvc02, setup.Logger);
Console.WriteLine();
Console.WriteLine("DONE -- SCRAPING");
Console.WriteLine();