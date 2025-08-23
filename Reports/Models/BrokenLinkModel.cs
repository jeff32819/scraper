namespace Reports.Models;

public class BrokenLinkModel
{


    public List<LinkModel> Links { get; set; } = new();
    public class LinkModel
    {
        public int Id { get; set; }
        public int StatusCode { get; set; }
        public string RawLink { get; set; }
        public string ScrapeUri { get; set; }
        public List<Page> Pages { get; set; } = new();
    }

    public class Page
    {
        public string PageUrl { get; set; }
        public string OuterHtml { get; set; }
    }
}