using NLog;


namespace ScraperCode
{
    public class NLogger(Logger logger)
    {
        private Logger Logger { get; } = logger;

        public void Info(string message, int indent = 0)
        {
            var dt = DateTime.Now;
            if (indent > 0)
            {
                message = $"{new string(' ', indent * 2)}{message}";
            }

            Logger.Info($"{dt:HH:mm:ss} {message}");
        }
    }
}
