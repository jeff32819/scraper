// ReSharper disable ConvertToPrimaryConstructor
namespace ScraperCode;

public class UriSections
{
    public UriSections(Uri uri)
    {
        Uri = uri;
    }
    public UriSections(string link)
    {
        Uri = new Uri(link);
    }
    public Uri Uri { get; set; }
    public string Scheme => Uri.Scheme;
    public string SchemeHost => Uri.GetLeftPart(UriPartial.Authority);
    public string SchemeHostPath => Uri.GetLeftPart(UriPartial.Path);
    public string SchemeHostPathQuery => Uri.GetLeftPart(UriPartial.Query);
    public string SchemeHostPathQueryFragment => Uri.AbsoluteUri;
}