using AngleSharp;

using HtmlAgilityPack;

using Jeff32819DLL.MiscCore20;

using Microsoft.EntityFrameworkCore;

using ScraperCode.Models;

namespace ScraperCode;

/// <summary>
///     Scrape request
/// </summary>
public static class WebRequest
{
    /// <summary>
    ///     Scrape type
    /// </summary>
    public enum ScrapeMethod
    {
        [ScrapeMethod("html-agility-pack")] HtmlAgilityPack,

        [ScrapeMethod("angle-sharp")] AngleSharp,

        [ScrapeMethod("httpclient")] HttpClient
    }

    /// <summary>
    ///     Scrapes a web page based on the scrape method enum value as a string (custom attribute value)
    /// </summary>
    /// <param name="scrapeMethod"></param>
    /// <param name="url"></param>
    /// <param name="headersOnly"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<IScrapeRequest> Parse(string scrapeMethod, string url, bool headersOnly)
    {
        var scrapeMethodEnum = EnumExtensions.GetEnumByAttributeValue<ScrapeMethod, ScrapeMethodAttribute>(x => x.Value == scrapeMethod);
        if (scrapeMethodEnum == null)
        {
            throw new Exception("Scrape enum not found");
        }

        return await Parse((ScrapeMethod)scrapeMethodEnum, url, headersOnly);
    }

    /// <summary>
    ///     Scrapes a web page based on the scrape method enum value
    /// </summary>
    /// <param name="scrapeMethod"></param>
    /// <param name="url"></param>
    /// <param name="headersOnly"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<IScrapeRequest> Parse(ScrapeMethod scrapeMethod, string url, bool headersOnly)
    {
        try
        {
            switch (scrapeMethod)
            {
                case ScrapeMethod.HtmlAgilityPack:
                    return await UsingHtmlAgilityPack(url, headersOnly);
                case ScrapeMethod.AngleSharp:
                    return await UsingAngleSharp(url, headersOnly);
                case ScrapeMethod.HttpClient:
                    return await GetFromHttpClient(url, headersOnly);
                default:
                    throw new ArgumentOutOfRangeException(nameof(scrapeMethod), scrapeMethod, null);
            }
        }
        catch (Exception ex)
        {
            return new ScrapeRequest
            {
                Url = url,
                Html = ex.Message,
                StatusCode = "ERROR"
            };
        }
    }

    private static async Task<IScrapeRequest> GetFromHttpClient(string url, bool headersOnly)
    {
        var httpCompletionOption = headersOnly ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/74.0.3729.1235");
        client.Timeout = TimeSpan.FromSeconds(30); // Set timeout to 30 seconds
        var httpResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        // jeff2do // maybe use later // response.EnsureSuccessStatusCode();  // Throws if not 2xx

        return await HttpClientHelper.GetAsync(new Uri(url));
    }

    /// <summary>
    ///     Does not do AJAX
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headersOnly"></param>
    /// <returns></returns>
    private static async Task<IScrapeRequest> UsingHtmlAgilityPack(string url, bool headersOnly)
    {
        var web = new HtmlWeb
        {
            Timeout = 10_000,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/74.0.3729.1235",
            PostResponse = (request, response) => { _ = response.ContentType; }
        };


        //Debug.Print($"url = {url}");
        //var tcs = new TaskCompletionSource<HttpWebResponse>();
        //var web = new HtmlWeb
        //{
        //    Timeout = (10 * 1000), // 10 seconds
        //    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/74.0.3729.1235"
        //};
        //web.PostResponse = (request, response) =>
        //{
        //    tcs.SetResult(response);
        //};
        //var response = await tcs.Task;
        //var contentType = response.ContentType;


        try
        {
            var extension = url.Split('.').Last().ToLower();
            switch (extension)
            {
                case "pdf":
                    return new ScrapeRequest
                    {
                        Html = "PDF",
                        StatusCode = "NOT_A_PAGE",
                        Url = url
                    };
                case "exe":
                    return new ScrapeRequest
                    {
                        Html = "exe",
                        StatusCode = "NOT_A_PAGE",
                        Url = url
                    };
                case "mp4":
                    return new ScrapeRequest
                    {
                        Html = "mp4",
                        StatusCode = "NOT_A_PAGE",
                        Url = url
                    };
                default:
                {
                    var doc = await web.LoadFromWebAsync(url);

                    return new ScrapeRequest
                    {
                        Html = doc.DocumentNode.OuterHtml,
                        StatusCode = web.StatusCode.ToString(),
                        Url = url
                    };
                }
            }
        }
        catch (Exception ex)
        {
            return new ScrapeRequest
            {
                Html = ex.Message,
                StatusCode = "ERROR",
                Url = url
            };
        }
    }


    /// <summary>
    ///     Does not have a status code
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headersOnly"></param>
    /// <returns></returns>
    private static async Task<IScrapeRequest> UsingAngleSharp(string url, bool headersOnly)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(url);
        return new ScrapeRequest
        {
            Html = document.DocumentElement.OuterHtml,
            StatusCode = "anglesharp-does-not-report", // does not report status code
            Url = url
        };
    }


    public class ScrapeMethodAttribute(string value) : Attribute
    {
        public string Value { get; } = value;
    }
}