using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace ScraperCode
{
    public class ResponseHeaderContainer
    {   
        public ResponseHeaderContainer(string jsonFromDatabase)
        {
            var tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<string>>>(jsonFromDatabase);
            if (tmp == null)
            {
                return;
            }
            Dictionary = tmp;
        }

        public ResponseHeaderContainer(HttpResponseHeaders? responseHeaders)
        {
            if (responseHeaders == null)
                return;

            foreach (var item in responseHeaders)
            {
                Dictionary.Add(item.Key, item.Value is List<string> list ? list : item.Value.ToList());
            }
        }
        public string ToJson => JsonConvert.SerializeObject(Dictionary, Formatting.None);
        public ResponseHeaderContainer(HttpContentHeaders? contentHeaders)
        {
            if (contentHeaders == null)
                return;

            foreach (var item in contentHeaders)
            {
                Dictionary.Add(item.Key, item.Value is List<string> list ? list : item.Value.ToList());
            }
        }

        public readonly Dictionary<string, IEnumerable<string>> Dictionary = new(StringComparer.OrdinalIgnoreCase);

        public void PrintToConsole()
        {
            foreach (var item in Dictionary)
            {
                Console.WriteLine(item.Key);
                foreach (var val in item.Value)
                {
                    Console.WriteLine($"\t- {val}");
                }
            }
        }

    }
}
