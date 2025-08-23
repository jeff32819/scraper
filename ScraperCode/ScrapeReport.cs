using BlazorTemplater;

using ScraperCode.Models;

namespace ScraperCode;

public static class ScrapeReport
{

    public static Task<string> ProcessRazor(DbService02 dbSvc, string url)
    {
        var uri = new Uri(url);
        var rs = dbSvc.ReportPagesForDomain(uri);
        var data = ReportData.Get(rs);
        var html = new ComponentRenderer<Reports.Components.BrokenLinkReport>()
            .Set(x => x.Data, data)
            .Render();
        return Task.FromResult(html);
    }
}