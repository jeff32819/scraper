using ScraperCode.Models;

using static ScraperCode.Models.HttpClientResponse;

// ReSharper disable InvertIf

namespace ScraperCode;

public static class HttpClientHelper
{
    public static async Task<IScrapeRequest> GetAsync(Uri url)
    {
        var response = await GetFromHttpClient(url);
        var wasRedirected = false;
        var redirectedStatusCode = 0;
        var redirectedFromUrl = "";

        if (response.IsRedirected)
        {
            wasRedirected = true;
            redirectedFromUrl = url.OriginalString;
            redirectedStatusCode = response.StatusCode;
            response = await GetFromHttpClient(response.RedirectedLocation);
        }

        return new ScrapeRequest
        {
            Url = response.Response?.RequestMessage?.RequestUri?.OriginalString ?? throw new Exception("RequestUri should not be null"),
            StatusCode = response.StatusCodeToString,
            ContentType = response.ContentType ?? "UNKNOWN",
            Html = await response.Content,
            ResponseHeaders = response.ResponseHeaders,
            ContentHeaders = response.ContentHeaders,
            WasRedirected = wasRedirected,
            RedirectStatusCode = redirectedStatusCode,
            RedirectedFromUrl = redirectedFromUrl
        };
    }



    public static async Task<HttpClientResponse> GetHttpClientResponse(Uri url)
    {
        var response = await GetFromHttpClient(url);
        if (response.IsRedirected)
        {
            response = await GetFromHttpClient(response.RedirectedLocation, new RedirectedModel
            {
                FromUrl = response.RequestUri,
                StatusCode = response.StatusCode
            });
        }
        return response;
    }



    private static async Task<HttpClientResponse> GetFromHttpClient(Uri url, RedirectedModel? redirectedFrom = null)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/74.0.3729.1235");
        client.Timeout = TimeSpan.FromSeconds(30); // Set timeout to 30 seconds
        var tmp = await client.GetAsync(url);
        return new HttpClientResponse(tmp, redirectedFrom);
    }
}