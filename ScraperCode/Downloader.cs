namespace ScraperCode;

public class Downloader
{
    public static async Task ExampleUsage()
    {
        const string fileUrl = "https://files.testfile.org/PDF/100MB-TESTFILE.ORG.pdf"; // Replace with a URL to a large file
        var destinationPath = Path.Combine(Path.GetTempPath(), "t:\\test-large-download.zip");

        var downloader = new Downloader();
        downloader.DownloadProgressChanged += ExampleDownloadProgress;
        //
        // next lines work great for inline code, instead of a method:
        //
        // downloader.DownloadProgressChanged += (sender, e) =>
        //    { Console.WriteLine($"Downloaded: {e.BytesReceived}/{e.TotalBytesToReceive} bytes ({e.ProgressPercentage}%)"); };

        Console.WriteLine($"Starting download of {fileUrl} to {destinationPath}...");

        var cts = new CancellationTokenSource();
        // You can use a timer or user input to cancel the download
        // For example, to cancel after 10 seconds:
        // cts.CancelAfter(10000);

        try
        {
            await downloader.DownloadFileAsync(fileUrl, destinationPath, cts.Token);
            Console.WriteLine($"\nFile downloaded successfully to: {destinationPath}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Download was explicitly cancelled by the user or timeout.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError during download: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private static void ExampleDownloadProgress(object? sender, DownloadProgressChangedEventArgs e)
    {
        Console.WriteLine($"Downloaded: {e.BytesReceived}/{e.TotalBytesToReceive} bytes ({e.ProgressPercentage}%)");
    }

    public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

    public async Task DownloadFileAsync(string url, string destinationFilePath, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        try
        {
            // Send the request and get the response headers immediately
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP status is an error

            var totalBytes = response.Content.Headers.ContentLength;
            long totalBytesRead = 0;
            var buffer = new byte[8192]; // Buffer size (8KB)

            // Open the destination file for writing
            await using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Get the response content stream
                await using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalBytesRead += bytesRead;

                        // Report progress
                        if (totalBytes.HasValue)
                        {
                            var progressPercentage = (int)((double)totalBytesRead / totalBytes.Value * 100);
                            OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(progressPercentage, totalBytesRead, totalBytes.Value));
                        }
                        else
                        {
                            // If Content-Length is not available, report bytes downloaded
                            OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(0, totalBytesRead, 0));
                        }
                    }
                }
            }

            OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(100, totalBytesRead, totalBytes ?? totalBytesRead)); // Ensure 100% is reported
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Download was cancelled.");
            // Clean up partially downloaded file if necessary
            if (File.Exists(destinationFilePath))
            {
                File.Delete(destinationFilePath);
            }

            throw; // Re-throw to propagate cancellation
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP request error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            throw;
        }
    }

    protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
    {
        DownloadProgressChanged?.Invoke(this, e);
    }


    public class DownloadProgressChangedEventArgs : EventArgs
    {
        public DownloadProgressChangedEventArgs(int progressPercentage, long bytesReceived, long totalBytesToReceive)
        {
            ProgressPercentage = progressPercentage;
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }

        public int ProgressPercentage { get; }
        public long BytesReceived { get; }
        public long TotalBytesToReceive { get; }
    }
}