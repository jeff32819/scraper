namespace ScraperCode.Models;

public class HttpClientResponseContainer
{
    public HttpClientResponse HttpClientResponse { get; set; }
    public string ErrorMessage { get; set; }
    public Uri RequestUri { get; set; }
    public bool IsRedirected { get; set; }
    public int StatusCode { get; set; }
}