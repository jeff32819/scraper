namespace ScraperCode
{
    public static class Ext
    {
        public static bool IsRedirectStatusCode(this int statusCode)
        {
            return ((int)statusCode >= 300 && (int)statusCode < 400);
        }
    }
}
