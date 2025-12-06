using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace SWGSurvivors_Patcher
{
    public class GitHubUpdateService
    {
        private const string GITHUB_REPO_OWNER = "SWG-Survivors";
        private const string GITHUB_REPO_NAME = "launcher";
        private const string LAUNCHER_ASSET_NAME = "SWGSurvivors-Patcher.exe";

        private readonly HttpClient httpClient;

        public GitHubUpdateService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            // GitHub API requires User-Agent header
            if (!this.httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                this.httpClient.DefaultRequestHeaders.Add("User-Agent", "SWGSurvivors-Patcher");
            }
        }

        public async Task<UpdateInfo?> CheckForUpdate(int timeoutSeconds = 5)
        {
            // NOTE: This method throws exceptions for connection errors
            // Only returns null when legitimately no update is available

            var apiUrl = $"https://api.github.com/repos/{GITHUB_REPO_OWNER}/{GITHUB_REPO_NAME}/releases/latest";

            // Use a cancellation token for timeout
            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                var json = await httpClient.GetStringAsync(apiUrl, cts.Token);
                var release = JsonSerializer.Deserialize<GitHubRelease>(json);

                if (release == null || string.IsNullOrEmpty(release.tag_name))
                    return null;

                // Parse version from tag (e.g., "v1.2.0" -> "1.2.0")
                var tagVersion = release.tag_name.TrimStart('v', 'V');
                if (!Version.TryParse(tagVersion, out var latestVersion))
                    return null;

                // Get current version
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (currentVersion == null)
                    return null;

                // Compare versions (only major.minor.build, ignore revision)
                var current = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);
                var latest = new Version(latestVersion.Major, latestVersion.Minor, latestVersion.Build);

                if (latest <= current)
                    return null; // Already up to date

                // Find launcher asset in release
                if (release.assets == null)
                    return null;

                foreach (var asset in release.assets)
                {
                    if (asset.name == LAUNCHER_ASSET_NAME)
                    {
                        return new UpdateInfo
                        {
                            CurrentVersion = current.ToString(),
                            LatestVersion = latest.ToString(),
                            DownloadUrl = asset.browser_download_url ?? string.Empty,
                            FileSize = asset.size,
                            ReleaseNotes = release.body ?? "No release notes available"
                        };
                    }
                }

                return null; // Launcher asset not found in release
            }
        }

        public async Task<bool> DownloadUpdate(UpdateInfo updateInfo, string destinationPath, IProgress<int>? progress = null)
        {
            try
            {
                using (var response = await httpClient.GetAsync(updateInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? updateInfo.FileSize;
                    var downloadedBytes = 0L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            if (totalBytes > 0)
                            {
                                var percent = (int)((double)downloadedBytes / totalBytes * 100);
                                progress?.Report(percent);
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                // Clean up failed download
                if (File.Exists(destinationPath))
                {
                    try { File.Delete(destinationPath); } catch { }
                }
                return false;
            }
        }

        public async Task<string> CalculateSHA256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = await Task.Run(() => sha256.ComputeHash(stream));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public class UpdateInfo
    {
        public string CurrentVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public long FileSize { get; set; }
        public string ReleaseNotes { get; set; } = "";
    }

    // GitHub API response models
    public class GitHubRelease
    {
        public string? tag_name { get; set; }
        public string? name { get; set; }
        public string? body { get; set; }
        public GitHubAsset[]? assets { get; set; }
    }

    public class GitHubAsset
    {
        public string? name { get; set; }
        public string? browser_download_url { get; set; }
        public long size { get; set; }
    }
}
