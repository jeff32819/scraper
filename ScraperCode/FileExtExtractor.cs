namespace ScraperCode;

public class FileExtExtractor
{
    public FileExtExtractor(string link)
    {
        Process(new Uri(link));
    }

    public FileExtExtractor(Uri uri)
    {
        Process(uri);
    }

    public string Extension { get; private set; } = "";

    private void Process(Uri uri)
    {
        var ext = Path.GetExtension(uri.GetLeftPart(UriPartial.Path));
        Extension = string.IsNullOrEmpty(ext) ? "" : ext.TrimStart('.').ToLower();
    }
}