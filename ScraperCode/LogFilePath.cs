namespace ScraperCode
{
    public class LogFilePath
    {
        public LogFilePath(string folder)
        {
            Folder = folder;
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }
        }
        public string LinkFileFolder => Path.Combine(Folder, "links-to-parse");
        public string Log => Path.Combine(Folder, "log.txt");
        public string Folder { get; set; }
    }
}
