namespace ScraperCode.Models;

public class HttpClientResponseContainer
{
    public HttpClientResponse HttpClientResponse { get; set; } = null!;
    public string ErrorMessage { get; set; } = "";
    public Uri RequestUri { get; set; } = null!;
    public bool IsRedirected { get; set; }
    public int StatusCode { get; set; }
}