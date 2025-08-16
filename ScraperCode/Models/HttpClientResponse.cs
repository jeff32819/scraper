using System.Net;
using Newtonsoft.Json;
// ReSharper disable UnusedMember.Global

namespace ScraperCode.Models;

public class HttpClientResponse
{
    public HttpClientResponse(HttpResponseMessage? response, HttpCompletionOption completionOption)
    {
        Response = response ?? throw new ArgumentNullException(nameof(response), "HttpResponseMessage cannot be null.");
        Init();
        CompletionOption = completionOption;
    }
    public HttpCompletionOption CompletionOption { get; }
    public HttpResponseMessage Response { get; }

    public Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string ResponseHeadersToJson => JsonConvert.SerializeObject(ResponseHeaders, Formatting.None);
    public string ContentHeadersToJson => JsonConvert.SerializeObject(ContentHeaders, Formatting.None);
    public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsWebPage =>
        !string.IsNullOrEmpty(ContentType) && (ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) ||
                                               ContentType.StartsWith("application/xhtml+xml", StringComparison.OrdinalIgnoreCase));
    public Task<string> Content => Response.Content.ReadAsStringAsync();

    public string? ContentType => Response.Content.Headers.ContentType?.MediaType;
    public HttpStatusCode StatusCode => Response.StatusCode;
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
}