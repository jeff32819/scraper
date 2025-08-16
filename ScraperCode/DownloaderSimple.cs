namespace ScraperCode;

/// <summary>
/// Code from MS Copilot
/// This is a good version for a console app, as it just passes a IProgress object.
/// </summary>
public static class DownloaderSimple
{
    /// <summary>
    /// Example usage of the downloader
    /// </summary>
    /// <returns></returns>
    public static async Task ExampleUsage()
    {
        var progress = new Progress<double>(p => Console.WriteLine($"Progress: {p:P1}"));
        await DownloadFileAsync("https://files.testfile.org/PDF/100MB-TESTFILE.ORG.pdf", "t:\\test-large-download.zip", progress);
    }
    /// <summary>
    /// Download file
    /// </summary>
    /// <param name="url"></param>
    /// <param name="destinationPath"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static async Task DownloadFileAsync(string url, string destinationPath, IProgress<double> progress)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1 && progress != null;

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long totalRead = 0;
        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;
            if (canReportProgress)
            {
                progress?.Report((double)totalRead / totalBytes);
            }
        }
    }
}