using System.Net.WebSockets;
using System.Reflection;
using CodeBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ScraperCode;
using ScraperCode.DbCtx;




var setup = ScraperCode.HostBuilderFactory.Create();



// await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://business.lakenonacc.org");
//var result1 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://sebastianmoving.net");
//var result2 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://www.sebastianmoving.net");



var result1 = await ConsoleAppForTesting.Test.GetFromWeb(setup.DbService1, "https://jeff32819.com");


Console.WriteLine();