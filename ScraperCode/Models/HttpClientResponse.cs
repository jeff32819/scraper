using System.Net;

// ReSharper disable UnusedMember.Global

namespace ScraperCode.Models;

public class HttpClientResponse
{
    public HttpClientResponse(HttpResponseMessage? response, RedirectedModel? redirectedFrom = null)
    {
        Response = response ?? throw new ArgumentNullException(nameof(response), "HttpResponseMessage cannot be null.");
        RedirectedFrom = redirectedFrom;
        RequestUri = Response.RequestMessage?.RequestUri ?? throw new InvalidOperationException();
        ResponseHeaders = new ResponseHeaderContainer(Response.Headers);
        ContentHeaders = new ResponseHeaderContainer(Response.Content.Headers);
        CharsetParsed = new CharsetParser(Response);
    }

    public RedirectedModel? RedirectedFrom { get; set; }
    public Uri RequestUri { get; }
    public HttpResponseMessage Response { get; }

    public ResponseHeaderContainer ResponseHeaders { get; }
    public ResponseHeaderContainer ContentHeaders { get; }

    /// <summary>
    ///     Is the status code 3xx status code? Useful to see if it was redirected.
    /// </summary>
    public bool IsRedirected => (int)Response.StatusCode >= 300 && (int)Response.StatusCode < 400 && Response.Headers.Location != null;

    public Uri RedirectedLocation => Response.Headers.Location == null ? throw new InvalidOperationException("Redirected location is not available.") : Response.Headers.Location;

    public bool IsWebPage =>
        !string.IsNullOrEmpty(ContentType) && (ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) ||
                                               ContentType.StartsWith("application/xhtml+xml", StringComparison.OrdinalIgnoreCase));

    public string? ContentType => Response.Content.Headers.ContentType?.MediaType;
    public int StatusCode => (int)Response.StatusCode;
    public HttpStatusCode StatusCodeEnum => Response.StatusCode;
    public string StatusCodeToString => Response.StatusCode.ToString();


    /// <summary>
    ///     Charset parsed from the Content-Type header. Use this to read the content correctly.
    /// </summary>
    public CharsetParser CharsetParsed { get; }

    public async Task<string> GetContentAsync()
    {
        return !CharsetParsed.IsValid ? "" : await Response.Content.ReadAsStringAsync();
    }

    /// <summary>
    ///     Represents a model for a redirection, including the source URL and the HTTP status code.
    /// </summary>
    /// <remarks>
    ///     This class is typically used to describe redirection details, such as the originating URL and
    ///     the HTTP status code associated with the redirection.
    /// </remarks>
    public class RedirectedModel
    {
        public required Uri FromUrl { get; init; }
        public int StatusCode { get; init; }
    }
}