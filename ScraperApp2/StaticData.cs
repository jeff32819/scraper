using ScraperCode;

namespace ScraperApp2
{
    public static class StaticData
    {
        public static void Update(DbService db)
        {
            var list = new List<string>
            {
                "",
                "asp",
                "aspx",
                "htm",
                "html",
                "php",
                "shtml",
                "xhtml",
            };
            foreach (var item in list)
            {
                db.FileTypeToScrapeAdd(item);
            }
        }
    }
}
