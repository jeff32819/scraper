using Microsoft.EntityFrameworkCore;

using ScraperCode.DbCtx;

namespace ScraperCode
{
    public class DbService02
    {
        public DbService02(WebScraperContext db)
        {
            DbCtx = db;
            DbConnString = DbCtx.Database.GetConnectionString() ?? throw new Exception("No database connection string");
        }

        public string DbConnString { get; }
        public WebScraperContext DbCtx { get; }
    }
}
