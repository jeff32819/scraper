using ScraperCode.Models;

using static ScraperCode.Models.HttpClientResponse;

// ReSharper disable InvertIf

namespace ScraperCode;

public static class HttpClientHelper
{
    public static async Task<IScrapeRequest> GetAsync(Uri url)
    {
        var responseContainer = await GetFromHttpClient(url);
        var wasRedirected = false;
        var redirectedStatusCode = 0;
        var redirectedFromUrl = "";

        if (responseContainer.HttpClientResponse.IsRedirected)
        {
            wasRedirected = true;
            redirectedFromUrl = url.OriginalString;
            redirectedStatusCode = responseContainer.HttpClientResponse.StatusCode;
            responseContainer = await GetFromHttpClient(responseContainer.HttpClientResponse.RedirectedLocation);
        }

        return new ScrapeRequest
        {
            Url = responseContainer.HttpClientResponse.Response?.RequestMessage?.RequestUri?.OriginalString ?? throw new Exception("RequestUri should not be null"),
            StatusCode = responseContainer.HttpClientResponse.StatusCodeToString,
            ContentType = responseContainer.HttpClientResponse.ContentType ?? "UNKNOWN",
            Html = await responseContainer.HttpClientResponse.Content,
            ResponseHeaders = responseContainer.HttpClientResponse.ResponseHeaders,
            ContentHeaders = responseContainer.HttpClientResponse.ContentHeaders,
            WasRedirected = wasRedirected,
            RedirectStatusCode = redirectedStatusCode,
            RedirectedFromUrl = redirectedFromUrl
        };
    }



    public static async Task<HttpClientResponseContainer> GetHttpClientResponse(Uri url)
    {
        var responseContainer = await GetFromHttpClient(url);
        if (responseContainer.IsRedirected)
        {
            responseContainer = await GetFromHttpClient(responseContainer.HttpClientResponse.RedirectedLocation, new RedirectedModel
            {
                FromUrl = responseContainer.RequestUri,
                StatusCode = responseContainer.HttpClientResponse.StatusCode
            });
        }
        return responseContainer;
    }



    private static async Task<HttpClientResponseContainer> GetFromHttpClient(Uri url, RedirectedModel? redirectedFrom = null)
    {

        var rv = new HttpClientResponseContainer
        {
            RequestUri = url
        };
        try
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/74.0.3729.1235");
            client.Timeout = TimeSpan.FromSeconds(30); // Set timeout to 30 seconds
            var tmp = await client.GetAsync(url);
            rv.HttpClientResponse = new HttpClientResponse(tmp, redirectedFrom);
            rv.IsRedirected = rv.HttpClientResponse.IsRedirected;
            rv.Success = true;
        }
        catch (Exception ex)
        {
            rv.ErrorMessage = ex.Message;
        }

        return rv;
    }
}