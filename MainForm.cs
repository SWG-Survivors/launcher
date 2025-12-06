using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWGSurvivors_Patcher
{
    public partial class MainForm : Form
    {
        private const string MANIFEST_URL = "http://fileserver.swgsurvivors.com/swgs/updates/manifest.json";
        private const string STATUS_URL = "http://fileserver.swgsurvivors.com/status/server_status.php";
        private readonly string workingDirectory;
        private readonly HttpClient httpClient;
        private LauncherSettings settings;

        // Statistics
        private int filesVerified = 0;
        private int filesDownloaded = 0;
        private int filesFailed = 0;
        private long bytesDownloaded = 0;
        private List<string> failedFiles = new List<string>();
        private List<FileInfo> filesToDownload = new List<FileInfo>();

        // UI Controls
        private Label titleLabel = null!;
        private Label statusLabel = null!;
        private Label activeServerLabel = null!;
        private Label serverStatusLabel = null!;
        private Label serverUptimeLabel = null!;
        private Label playerOnlineLabel = null!;
        private Label playerPeakLabel = null!;
        private Label overallProgressLabel = null!;
        private ProgressBar overallProgressBar = null!;
        private Label fileProgressLabel = null!;
        private ProgressBar fileProgressBar = null!;
        private RichTextBox logTextBox = null!;
        private Button startButton = null!;
        private Button launchGameButton = null!;
        private Button closeButton = null!;
        private PictureBox logoPictureBox = null!;
        private Button settingsButton = null!;

        public MainForm()
        {
            // Get the directory where the executable is located
            workingDirectory = AppContext.BaseDirectory;

            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(10);

            // Load settings
            settings = LauncherSettings.Load();

            InitializeComponent();
            LoadServerStatus();
        }

        private void InitializeComponent()
        {
            // Get version from assembly
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var versionString = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "";

            this.Text = $"SWGSurvivors Delta Patcher {versionString}";
            this.Size = new Size(750, 625);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int yPos = 10;

            // Title
            titleLabel = new Label
            {
                Text = "SWGSurvivors Delta Patcher",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(titleLabel);

            // Logo (top right corner) - embedded resource
            try
            {
                var resourceName = "SWGSurvivors_Patcher.logo.png";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        logoPictureBox = new PictureBox
                        {
                            Image = Image.FromStream(stream),
                            SizeMode = PictureBoxSizeMode.Zoom,  // Maintains aspect ratio
                            Size = new Size(80, 80),  // Square dimensions for square logo
                            Location = new Point(630, yPos + 5),  // Add spacing from top
                            BackColor = Color.Transparent
                        };
                        this.Controls.Add(logoPictureBox);
                    }
                }
            }
            catch
            {
                // Logo couldn't load - continue without it
            }

            yPos += 45;

            // Server status information bar - First line
            int statusBarY = yPos;
            int xOffset = 20;

            // ACTIVE SERVER
            Label activeServerTitle = new Label
            {
                Text = "Active Server:",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(xOffset, statusBarY),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(activeServerTitle);

            activeServerLabel = new Label
            {
                Text = "LOADING",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(activeServerTitle.Right + 5, statusBarY),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(activeServerLabel);
            xOffset = activeServerLabel.Right + 75;

            // STATUS
            Label statusTitle = new Label
            {
                Text = "Status:",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(xOffset, statusBarY),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(statusTitle);

            serverStatusLabel = new Label
            {
                Text = "LOADING",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(statusTitle.Right + 5, statusBarY),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent
            };
            this.Controls.Add(serverStatusLabel);
            xOffset = serverStatusLabel.Right + 25;

            // UPTIME
            Label uptimeTitle = new Label
            {
                Text = "Uptime:",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(xOffset, statusBarY),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(uptimeTitle);

            serverUptimeLabel = new Label
            {
                Text = "0",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(uptimeTitle.Right + 5, statusBarY),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(serverUptimeLabel);

            // Second line for ONLINE and PEAK
            yPos += 20;
            xOffset = 20;

            // ONLINE
            Label onlineTitle = new Label
            {
                Text = "Online:",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(xOffset, yPos),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(onlineTitle);

            playerOnlineLabel = new Label
            {
                Text = "0",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(onlineTitle.Right + 5, yPos),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(playerOnlineLabel);
            xOffset = playerOnlineLabel.Right + 30;

            // PEAK
            Label peakTitle = new Label
            {
                Text = "Peak:",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(xOffset, yPos),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(peakTitle);

            playerPeakLabel = new Label
            {
                Text = "0",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(peakTitle.Right + 5, yPos),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };
            this.Controls.Add(playerPeakLabel);

            yPos += 40;

            // Status Label
            statusLabel = new Label
            {
                Text = "Ready",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, yPos),
                ForeColor = Color.Blue
            };
            this.Controls.Add(statusLabel);
            yPos += 25;

            // Overall Progress Label
            overallProgressLabel = new Label
            {
                Text = "Overall Progress: 0%",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(overallProgressLabel);
            yPos += 25;

            // Overall Progress Bar
            overallProgressBar = new ProgressBar
            {
                Location = new Point(20, yPos),
                Size = new Size(690, 25),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(overallProgressBar);
            yPos += 35;

            // File Progress Label
            fileProgressLabel = new Label
            {
                Text = "File Progress: 0%",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(fileProgressLabel);
            yPos += 25;

            // File Progress Bar
            fileProgressBar = new ProgressBar
            {
                Location = new Point(20, yPos),
                Size = new Size(690, 25),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(fileProgressBar);
            yPos += 35;

            // Log Label
            Label logLabel = new Label
            {
                Text = "Activity Log:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(logLabel);
            yPos += 25;

            // Log TextBox
            logTextBox = new RichTextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(690, 240),
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.White
            };
            this.Controls.Add(logTextBox);
            yPos += 250;

            // Buttons (3 buttons, centered)
            int buttonWidth = 140;
            int buttonSpacing = 15;
            int totalWidth = (buttonWidth * 3) + (buttonSpacing * 2);
            int startX = (750 - totalWidth) / 2;

            // Check if game directory exists (prevents patching in wrong location)
            var gameExePath = Path.Combine(workingDirectory, "SWGEmu.exe");
            bool gameExists = File.Exists(gameExePath);

            startButton = new Button
            {
                Text = gameExists ? "Start Patching" : "Game not found",
                Size = new Size(buttonWidth, 40),
                Location = new Point(startX, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = gameExists
            };
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

            // Launch Game button
            launchGameButton = new Button
            {
                Text = gameExists ? "Launch Game" : "Game not found",
                Size = new Size(buttonWidth, 40),
                Location = new Point(startX + buttonWidth + buttonSpacing, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = gameExists
            };
            launchGameButton.Click += LaunchGameButton_Click;
            this.Controls.Add(launchGameButton);

            closeButton = new Button
            {
                Text = "Close",
                Size = new Size(buttonWidth, 40),
                Location = new Point(startX + (buttonWidth + buttonSpacing) * 2, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = true
            };
            closeButton.Click += (s, e) => this.Close();
            this.Controls.Add(closeButton);

            // Settings button (bottom right corner)
            settingsButton = new Button
            {
                Text = "\u2699",  // Gear/cog emoji
                Size = new Size(40, 40),
                Location = new Point(this.ClientSize.Width - 60, yPos - 2),  // Adjust up by 2px for visual alignment
                Font = new Font("Arial", 24),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Enabled = true
            };
            settingsButton.FlatAppearance.BorderSize = 0;
            settingsButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            settingsButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            settingsButton.Click += SettingsButton_Click;
            this.Controls.Add(settingsButton);
        }

        private async void LoadServerStatus()
        {
            try
            {
                var json = await httpClient.GetStringAsync(STATUS_URL);
                var serverStatus = JsonSerializer.Deserialize<ServerStatus>(json);

                if (serverStatus != null)
                {
                    // Update Active Server name
                    activeServerLabel.Text = serverStatus.name?.ToUpper() ?? "UNKNOWN";

                    // Update Status
                    serverStatusLabel.Text = serverStatus.status?.ToUpper() ?? "UNKNOWN";

                    // Change color based on status
                    if (serverStatus.status?.ToLower() == "online")
                    {
                        serverStatusLabel.ForeColor = Color.White;
                        serverStatusLabel.BackColor = Color.Green;
                    }
                    else if (serverStatus.status?.ToLower() == "offline")
                    {
                        serverStatusLabel.ForeColor = Color.White;
                        serverStatusLabel.BackColor = Color.Red;
                    }
                    else
                    {
                        serverStatusLabel.ForeColor = Color.White;
                        serverStatusLabel.BackColor = Color.Orange;
                    }

                    // Update Uptime (convert seconds to readable format)
                    if (!string.IsNullOrEmpty(serverStatus.uptime) && long.TryParse(serverStatus.uptime, out long uptimeSeconds))
                    {
                        TimeSpan uptime = TimeSpan.FromSeconds(uptimeSeconds);
                        serverUptimeLabel.Text = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
                    }
                    else
                    {
                        serverUptimeLabel.Text = serverStatus.uptime ?? "0";
                    }

                    // Update Online players
                    playerOnlineLabel.Text = serverStatus.connected ?? "0";

                    // Update Peak players
                    playerPeakLabel.Text = serverStatus.max ?? "0";
                }
                else
                {
                    activeServerLabel.Text = "ERROR";
                    serverStatusLabel.Text = "PARSE ERROR";
                    serverStatusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                activeServerLabel.Text = "ERROR";
                serverStatusLabel.Text = "LOAD ERROR";
                serverStatusLabel.ForeColor = Color.Red;
                Log($"Failed to load server status: {ex.Message}", LogLevel.Error);
            }

            // Auto-patch on startup if enabled
            if (settings.PatchAutomaticallyOnStartup && startButton.Enabled)
            {
                Log("Auto-patch enabled - starting patching automatically...");
                StartButton_Click(null, EventArgs.Empty);
            }
        }

        private void LaunchGameButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var gameExePath = Path.Combine(workingDirectory, "SWGEmu.exe");

                if (File.Exists(gameExePath))
                {
                    Log("Launching SWGEmu.exe...");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = gameExePath,
                        UseShellExecute = true,
                        WorkingDirectory = workingDirectory
                    });
                    Log("Game launched successfully - closing patcher", LogLevel.Success);

                    // Close the patcher after launching the game
                    this.Close();
                }
                else
                {
                    Log("SWGEmu.exe not found in current directory", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to launch game: {ex.Message}", LogLevel.Error);
            }
        }

        private async void StartButton_Click(object? sender, EventArgs e)
        {
            // Disable all buttons during patching
            startButton.Enabled = false;
            launchGameButton.Enabled = false;
            closeButton.Enabled = false;

            // Reset statistics for new run
            filesVerified = 0;
            filesDownloaded = 0;
            filesFailed = 0;
            bytesDownloaded = 0;
            failedFiles.Clear();
            filesToDownload.Clear();

            // Reset progress bars
            overallProgressBar.Value = 0;
            overallProgressLabel.Text = "Overall Progress: 0%";
            fileProgressBar.Value = 0;
            fileProgressLabel.Text = "File Progress: 0%";

            Log("============================================================");
            Log("SWGSurvivors Delta Patcher Started");
            Log($"Working directory: {workingDirectory}");
            Log("============================================================");

            await RunPatching();

            // Re-enable all buttons after patching
            startButton.Enabled = true;
            closeButton.Enabled = true;

            // Re-enable launch button only if game exists
            var gameExePath = Path.Combine(workingDirectory, "SWGEmu.exe");
            if (File.Exists(gameExePath))
            {
                launchGameButton.Enabled = true;
            }
        }

        private async Task RunPatching()
        {
            try
            {
                // Download manifest
                var manifest = await DownloadManifest();
                if (manifest == null)
                {
                    Log("Patching aborted due to manifest error", LogLevel.Error);
                    UpdateStatus("Patching failed");
                    return;
                }

                // Verify files
                await VerifyFiles(manifest);

                // Download files
                await DownloadFiles();

                // Complete
                Log("============================================================");
                if (filesFailed > 0)
                {
                    UpdateStatus("Patching completed with errors!");
                    Log("SUMMARY:", LogLevel.Warning);
                    Log($"  Files verified OK: {filesVerified}");
                    Log($"  Files downloaded successfully: {filesDownloaded}");
                    Log($"  Files FAILED to download: {filesFailed}", LogLevel.Error);
                    Log($"  Total bytes downloaded: {bytesDownloaded:N0}");
                    Log("");
                    Log("Failed files:", LogLevel.Error);
                    foreach (var file in failedFiles)
                    {
                        Log($"  - {file}", LogLevel.Error);
                    }
                    Log("");
                    Log("IMPORTANT: Close the game and run the patcher again to update these files!", LogLevel.Warning);
                }
                else
                {
                    UpdateStatus("Patching complete!");
                    Log("SUMMARY:", LogLevel.Success);
                    Log($"  Files verified OK: {filesVerified}");
                    Log($"  Files downloaded: {filesDownloaded}");
                    Log($"  Total bytes downloaded: {bytesDownloaded:N0}");
                }
                Log("============================================================");
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex.Message}", LogLevel.Error);
                UpdateStatus("Patching failed");
            }
        }

        private async Task<Manifest?> DownloadManifest()
        {
            Log($"Downloading manifest from {MANIFEST_URL}");
            UpdateStatus("Downloading manifest...");

            try
            {
                var json = await httpClient.GetStringAsync(MANIFEST_URL);
                var manifest = JsonSerializer.Deserialize<Manifest>(json);
                Log("Manifest downloaded successfully", LogLevel.Success);
                return manifest;
            }
            catch (Exception ex)
            {
                Log($"Failed to download manifest: {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        private async Task VerifyFiles(Manifest manifest)
        {
            Log("Starting file verification...");
            UpdateStatus("Verifying local files...");

            var files = manifest.files ?? new List<FileEntry>();
            var baseUrl = manifest.baseUrl ?? "";

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var progress = (int)((double)i / files.Count * 100);
                overallProgressBar.Value = progress;
                overallProgressLabel.Text = $"Verification Progress: {progress}%";
                Application.DoEvents();

                var localPath = Path.Combine(workingDirectory, file.path ?? "");

                // Check if file exists
                if (!File.Exists(localPath))
                {
                    Log($"Missing: {file.path}");
                    filesToDownload.Add(new FileInfo
                    {
                        Path = file.path ?? "",
                        Url = baseUrl + file.path,
                        Hash = file.hash ?? "",
                        Size = file.size
                    });
                    continue;
                }

                // Check file size
                var fileInfo = new System.IO.FileInfo(localPath);
                if (fileInfo.Length != file.size)
                {
                    Log($"Size mismatch: {file.path} (expected {file.size}, got {fileInfo.Length})");
                    filesToDownload.Add(new FileInfo
                    {
                        Path = file.path ?? "",
                        Url = baseUrl + file.path,
                        Hash = file.hash ?? "",
                        Size = file.size
                    });
                    continue;
                }

                // Verify hash
                var actualHash = await CalculateSHA256(localPath);
                if (actualHash != file.hash)
                {
                    Log($"Hash mismatch: {file.path}");
                    filesToDownload.Add(new FileInfo
                    {
                        Path = file.path ?? "",
                        Url = baseUrl + file.path,
                        Hash = file.hash ?? "",
                        Size = file.size
                    });
                }
                else
                {
                    filesVerified++;
                }
            }

            overallProgressBar.Value = 100;
            overallProgressLabel.Text = "Verification Progress: 100%";

            Log($"Verification complete: {filesVerified} files OK, {filesToDownload.Count} files need updating");
        }

        private async Task DownloadFiles()
        {
            if (filesToDownload.Count == 0)
            {
                Log("All files are up to date!", LogLevel.Success);
                return;
            }

            // Check for locked files
            var lockedFiles = new List<string>();
            foreach (var file in filesToDownload)
            {
                var localPath = Path.Combine(workingDirectory, file.Path);
                if (File.Exists(localPath) && IsFileLocked(localPath))
                {
                    lockedFiles.Add(file.Path);
                }
            }

            if (lockedFiles.Count > 0)
            {
                Log("============================================================", LogLevel.Warning);
                Log("WARNING: Some files are in use by another process!", LogLevel.Warning);
                Log("These files cannot be updated:", LogLevel.Warning);
                foreach (var file in lockedFiles)
                {
                    Log($"  - {file}", LogLevel.Warning);
                }
                Log("");
                Log("Please close the game and any other applications using these files.", LogLevel.Warning);
                Log("The patcher will attempt to download other files and skip locked ones.", LogLevel.Warning);
                Log("============================================================", LogLevel.Warning);
                Log("");
            }

            Log($"Downloading {filesToDownload.Count} files...");
            UpdateStatus("Downloading files...");

            for (int i = 0; i < filesToDownload.Count; i++)
            {
                var file = filesToDownload[i];
                var overallPercent = (int)((double)i / filesToDownload.Count * 100);
                overallProgressBar.Value = overallPercent;
                overallProgressLabel.Text = $"Overall Progress: {overallPercent}% ({i}/{filesToDownload.Count} files)";

                fileProgressBar.Value = 0;
                fileProgressLabel.Text = "File Progress: 0%";

                var success = await DownloadFile(file);
                if (!success)
                {
                    filesFailed++;
                    failedFiles.Add(file.Path);
                }
            }

            overallProgressBar.Value = 100;
            overallProgressLabel.Text = $"Overall Progress: 100% ({filesToDownload.Count}/{filesToDownload.Count} files)";
            fileProgressBar.Value = 100;
            fileProgressLabel.Text = "File Progress: 100%";

            if (filesFailed > 0)
            {
                Log($"Download completed with errors: {filesDownloaded} succeeded, {filesFailed} failed", LogLevel.Warning);
            }
            else
            {
                Log($"Download complete: {filesDownloaded} files downloaded ({bytesDownloaded:N0} bytes)", LogLevel.Success);
            }
        }

        private async Task<bool> DownloadFile(FileInfo file)
        {
            var localPath = Path.Combine(workingDirectory, file.Path);
            var tempPath = localPath + ".tmp";

            Log($"Downloading: {file.Path} ({file.Size:N0} bytes)");

            try
            {
                // Create parent directories
                var directory = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Download file
                using (var response = await httpClient.GetAsync(file.Url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? file.Size;
                    var downloadedBytes = 0L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            var percent = (int)((double)downloadedBytes / totalBytes * 100);
                            fileProgressBar.Value = Math.Min(percent, 100);
                            fileProgressLabel.Text = $"File Progress: {percent}% ({downloadedBytes:N0} / {totalBytes:N0} bytes)";
                            Application.DoEvents();
                        }
                    }
                }

                // Verify hash
                var downloadedHash = await CalculateSHA256(tempPath);
                if (downloadedHash != file.Hash)
                {
                    Log($"Hash verification failed for {file.Path}", LogLevel.Error);
                    File.Delete(tempPath);
                    return false;
                }

                // Move to final location
                try
                {
                    if (File.Exists(localPath))
                    {
                        File.Delete(localPath);
                    }
                    File.Move(tempPath, localPath);
                }
                catch (UnauthorizedAccessException)
                {
                    Log($"Cannot write to {file.Path}: File is in use by another process", LogLevel.Error);
                    File.Delete(tempPath);
                    return false;
                }
                catch (IOException ex)
                {
                    Log($"Cannot save {file.Path}: {ex.Message}", LogLevel.Error);
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                    return false;
                }

                filesDownloaded++;
                bytesDownloaded += file.Size;
                Log($"Downloaded: {file.Path}", LogLevel.Success);
                return true;
            }
            catch (Exception ex)
            {
                Log($"Failed to download {file.Path}: {ex.Message}", LogLevel.Error);
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                return false;
            }
        }

        private async Task<string> CalculateSHA256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = await Task.Run(() => sha256.ComputeHash(stream));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
            Application.DoEvents();
        }

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            string prefix = level switch
            {
                LogLevel.Error => "[ERROR] ",
                LogLevel.Success => "[SUCCESS] ",
                LogLevel.Warning => "[WARN] ",
                _ => "[INFO] "
            };

            Color color = level switch
            {
                LogLevel.Error => Color.Red,
                LogLevel.Success => Color.Green,
                LogLevel.Warning => Color.Orange,
                _ => Color.Black
            };

            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectionColor = color;
            logTextBox.AppendText(prefix + message + Environment.NewLine);
            logTextBox.SelectionColor = logTextBox.ForeColor;
            logTextBox.ScrollToCaret();
            Application.DoEvents();
        }

        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(settings);
            var result = settingsForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // Reload settings from the saved file
                settings = LauncherSettings.Load();
                Log("Settings saved successfully", LogLevel.Success);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Data models
    public class Manifest
    {
        public List<FileEntry>? files { get; set; }
        public string? baseUrl { get; set; }
        public string? lastUpdated { get; set; }
    }

    public class FileEntry
    {
        public string? path { get; set; }
        public long size { get; set; }
        public string? hash { get; set; }
    }

    public class FileInfo
    {
        public string Path { get; set; } = "";
        public string Url { get; set; } = "";
        public string Hash { get; set; } = "";
        public long Size { get; set; }
    }

    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    // Server status data model
    public class ServerStatus
    {
        public string? name { get; set; }
        public string? status { get; set; }
        public string? date { get; set; }
        public string? uptime { get; set; }
        public string? connected { get; set; }
        public string? max { get; set; }
        public string? total { get; set; }
        public string? deleted { get; set; }
    }
}
