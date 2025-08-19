var setup = ScraperCode.HostBuilderFactory.Create();



// await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://business.lakenonacc.org");
//var result1 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://sebastianmoving.net");
//var result2 = await ConsoleAppForTesting.Test.GetFromWeb(dbSvc, "https://www.sebastianmoving.net");


//var result1 = await ConsoleAppForTesting.Test.GetFromWeb(setup.DbSvc01, "https://jeff32819.com");


setup.DbSvc02.DbResetWithoutWarning();



var xxx = await  setup.DbSvc02.HostAdd(new Uri("https://novastardesign.com"), 100, "test");


Console.WriteLine();