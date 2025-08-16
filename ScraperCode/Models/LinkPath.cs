namespace ScraperCode.Models;

public class LinkPath
{
    public LinkPath(string txt)
    {
        OriginalValue = txt;
        CurrentValue = txt;
    }

    public string OriginalValue { get; }
    public string CurrentValue { get; set; }
}