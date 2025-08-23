using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
// ReSharper disable UnusedMember.Global

namespace ScraperCode.Models;

public class HttpClientResponse
{
    public HttpClientResponse(HttpResponseMessage? response, RedirectedModel? redirectedFrom = null)
    {
        Response = response ?? throw new ArgumentNullException(nameof(response), "HttpResponseMessage cannot be null.");
        RedirectedFrom = redirectedFrom;
        RequestUri = Response.RequestMessage?.RequestUri ?? throw new InvalidOperationException();
        Init();
    }

    public RedirectedModel? RedirectedFrom { get; set; }
    public Uri RequestUri { get; }
    public HttpResponseMessage Response { get; }

    public Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string ResponseHeadersToJson => JsonConvert.SerializeObject(ResponseHeaders, Formatting.None);
    public string ContentHeadersToJson => JsonConvert.SerializeObject(ContentHeaders, Formatting.None);
    public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Is the status code 3xx status code? Useful to see if it was redirected.
    /// </summary>
    public bool IsRedirected => ((int)Response.StatusCode >= 300 && (int)Response.StatusCode < 400 && Response.Headers.Location != null);

    public Uri RedirectedLocation => Response.Headers.Location == null ? throw new InvalidOperationException("Redirected location is not available.") : Response.Headers.Location;

    public bool IsWebPage =>
        !string.IsNullOrEmpty(ContentType) && (ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) ||
                                               ContentType.StartsWith("application/xhtml+xml", StringComparison.OrdinalIgnoreCase));



    public string? ContentType => Response.Content.Headers.ContentType?.MediaType;
    public int StatusCode => (int)Response.StatusCode;
    public HttpStatusCode StatusCodeEnum => Response.StatusCode;
    public string StatusCodeToString => Response.StatusCode.ToString();

    private void Init()
    {
        foreach (var header in Response.Headers)
        {
            ResponseHeaders[header.Key] = header.Value;
        }
        foreach (var header in Response.Content.Headers)
        {
            ContentHeaders[header.Key] = header.Value;
        }
    }

    public class RedirectedModel
    {
        public Uri FromUrl { get; set; }
        public int StatusCode { get; set; }
    }

    public async Task<string> GetContentAsync()
    {
        Debug.Print("GetContentAsync()");
        Debug.Print(RequestUri.ToString());

        var contentType = Response.Content.Headers.ContentType;
        var charset = contentType?.CharSet;

        // Normalize unsupported charset values
        if (!string.IsNullOrEmpty(charset))
        {
            // List of known invalid charsets to normalize
            var invalidCharsets = new[] { "utf8", "utf8mb4", "utf-8mb4" };
            if (invalidCharsets.Any(c => charset.Equals(c, StringComparison.OrdinalIgnoreCase)))
            {
                if (contentType != null && !string.IsNullOrEmpty(contentType.MediaType))
                {
                    var newContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType.MediaType)
                    {
                        CharSet = "utf-8"
                    };
                    Response.Content.Headers.ContentType = newContentType;
                }
                // else: do nothing, as we cannot set a new content type without a valid media type
            }
        }

        return await Response.Content.ReadAsStringAsync();
    }
}