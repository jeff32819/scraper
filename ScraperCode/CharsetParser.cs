using System.Text;

namespace ScraperCode
{
    public class CharsetParser
    {
        public CharsetParser(HttpResponseMessage httpResponse)
        {
            HttpResponse = httpResponse;
            var charset = HttpResponse.Content.Headers.ContentType?.CharSet;
            if (string.IsNullOrWhiteSpace(charset))
            {
                return;
            }
            RawEncoding = charset;
            var contentType = HttpResponse.Content.Headers.ContentType;
            try
            {
                Encoding = Encoding.GetEncoding(charset.Trim('"'));
            }
            catch (ArgumentException)
            {
                // Log here
            }
            // List of known invalid charsets to normalize
            var invalidCharsets = new[] { "utf8", "utf8mb4", "utf-8mb4" };
            if (!invalidCharsets.Any(c => charset.Equals(c, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            if (contentType == null || string.IsNullOrEmpty(contentType.MediaType))
            {
                return;
            }
            var newContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType.MediaType)
            {
                CharSet = "utf-8"
            };
            HttpResponse.Content.Headers.ContentType = newContentType;
            Encoding = Encoding.GetEncoding(newContentType.CharSet);
            EncodingWasFixed = true;
        }

        public HttpResponseMessage HttpResponse { get; }
        public bool EncodingWasFixed { get; set; }
        public bool IsValid => Encoding != null;
        public string RawEncoding { get; set; } = "";
        public Encoding? Encoding { get; set; }
    }
}
