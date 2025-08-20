// ReSharper disable ConvertToPrimaryConstructor
namespace ScraperCode;

public class UriSections
{
    public UriSections(Uri uri)
    {
        Uri = uri;
        IsValid = true;
    }

    public UriSections(string linkUrl)
    {
        try
        {
            Uri = new Uri(linkUrl, UriKind.RelativeOrAbsolute);
            IsValid = Uri.IsAbsoluteUri;
        }
        catch (UriFormatException ex)
        {
            Uri = null!;
            IsValid = false;
        }
    }
    public UriSections(string pageUrl, string linkUrl)
    {
        try
        {
            Uri = new Uri(linkUrl, UriKind.RelativeOrAbsolute);
            if (!Uri.IsAbsoluteUri)
            {
                Uri = new Uri(new Uri(pageUrl), linkUrl);
            }
            IsValid = true;
        }
        catch (UriFormatException ex)
        {
            Uri = null!;
            IsValid = false;
        }
    }

    public UriSections(Uri pageUrl, string linkUrl)
    {
        try
        {
            Uri = new Uri(linkUrl, UriKind.RelativeOrAbsolute);
            if (!Uri.IsAbsoluteUri)
            {
                Uri = new Uri(pageUrl, linkUrl);
            }
            IsValid = true;
        }
        catch (UriFormatException ex)
        {
            Uri = null!;
            IsValid = false;
        }
    }
    public UriSections(Uri pageUrl, Uri linkUrl)
    {
        try
        {
            if(linkUrl.IsAbsoluteUri)
            {
                Uri = linkUrl;
                IsValid = true;
                return;
            }
            Uri = new Uri(pageUrl, linkUrl);
            IsValid = true;
        }
        catch (UriFormatException ex)
        {
            Uri = null!;
            IsValid = false;
        }
    }

    public bool IsValid { get; }
    public Uri Uri { get; }
    public string Scheme => Uri.Scheme;
    public string SchemeHost => Uri.GetLeftPart(UriPartial.Authority);
    public string SchemeHostPath => Uri.GetLeftPart(UriPartial.Path);
    public string SchemeHostPathQuery => Uri.GetLeftPart(UriPartial.Query);
    public string SchemeHostPathQueryFragment => Uri.AbsoluteUri;
}