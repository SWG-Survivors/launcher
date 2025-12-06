using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWGSurvivors_Patcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run async check on UI thread to keep form responsive
            var shouldRestart = false;
            var updateCheckForm = new Form
            {
                Visible = false,
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized,
                Size = new System.Drawing.Size(0, 0),
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0
            };
            updateCheckForm.Load += async (s, e) =>
            {
                shouldRestart = await CheckForLauncherUpdateAsync();
                updateCheckForm.Close();
            };
            Application.Run(updateCheckForm);

            // Check for launcher updates before showing main form
            if (shouldRestart)
            {
                // Update was found and applied, application will restart
                return;
            }

            // No update needed, proceed with normal launcher
            Application.Run(new MainForm());
        }

        private static async Task<bool> CheckForLauncherUpdateAsync()
        {
            UpdateForm? updateForm = null;

            try
            {
                updateForm = new UpdateForm();
                updateForm.Show();
                updateForm.SetIndeterminate(true);
                updateForm.SetStatus("Checking for launcher updates...");
                updateForm.Refresh(); // Force UI update

                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) }) // Shorter timeout for HttpClient
                {
                    var updateService = new GitHubUpdateService(httpClient);

                    // Check for updates with 5-second timeout
                    updateForm.SetStatus("Connecting to GitHub...");
                    updateForm.Refresh(); // Force UI update

                    UpdateInfo? updateInfo = null;
                    try
                    {
                        // Create a task with timeout
                        var checkTask = updateService.CheckForUpdate(timeoutSeconds: 5);
                        var timeoutTask = Task.Delay(5500); // Slightly longer than service timeout
                        var completedTask = await Task.WhenAny(checkTask, timeoutTask);

                        if (completedTask == timeoutTask)
                        {
                            // Timeout occurred
                            updateForm.SetStatus("Cannot connect to GitHub");
                            updateForm.SetVersionInfo("Continuing without update...");
                            await Task.Delay(3000);
                            updateForm.Close();
                            return false;
                        }

                        updateInfo = await checkTask;
                    }
                    catch (TaskCanceledException)
                    {
                        // Connection timeout
                        updateForm.SetStatus("Cannot connect to GitHub - Connection Timed Out");
                        updateForm.SetVersionInfo("Continuing without update...");
                        await Task.Delay(3000);
                        updateForm.Close();
                        return false;
                    }
                    catch (HttpRequestException)
                    {
                        // Network error
                        updateForm.SetStatus("Cannot connect to GitHub - HTTP Request Exception");
                        updateForm.SetVersionInfo("Continuing without update...");
                        await Task.Delay(3000);
                        updateForm.Close();
                        return false;
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation
                        updateForm.SetStatus("Cannot connect to GitHub - Operation Canceled");
                        updateForm.SetVersionInfo("Continuing without update...");
                        await Task.Delay(3000);
                        updateForm.Close();
                        return false;
                    }

                    if (updateInfo == null)
                    {
                        // No update available
                        updateForm.SetStatus("Launcher is up to date");
                        updateForm.SetVersionInfo("No updates available");
                        await Task.Delay(3000);
                        updateForm.Close();
                        return false;
                    }

                    // Update available - close form and prompt user
                    updateForm.Close();

                    var result = MessageBox.Show(
                        $"A new launcher version is available!\n\nCurrent version: v{updateInfo.CurrentVersion}\nNew version: v{updateInfo.LatestVersion}\n\nWould you like to download and install the update now?\n\nThe launcher will restart after downloading.",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        // User declined - continue with current version
                        return false;
                    }

                    // User accepted - show form again and download
                    updateForm = new UpdateForm();
                    updateForm.Show();
                    updateForm.SetVersionInfo($"Downloading v{updateInfo.LatestVersion}");
                    updateForm.SetIndeterminate(false);
                    updateForm.SetProgress(0);

                    var currentExe = Application.ExecutablePath;
                    var newExe = currentExe + ".new";

                    var progress = new Progress<int>(percent =>
                    {
                        updateForm.SetProgress(percent);
                        updateForm.SetStatus($"Downloading update... {percent}%");
                    });

                    var downloadSuccess = await updateService.DownloadUpdate(updateInfo, newExe, progress);

                    if (!downloadSuccess)
                    {
                        updateForm.SetStatus("Download failed - continuing with current version");
                        await Task.Delay(2000);
                        updateForm.Close();
                        return false;
                    }

                    // Download complete - apply update and restart
                    updateForm.SetProgress(100);
                    updateForm.SetStatus("Restarting to apply update...");
                    await Task.Delay(1500);
                    updateForm.Close();

                    // Create batch file and restart
                    ApplyUpdateAndRestart(currentExe, newExe);
                    return true; // Signal that we're restarting
                }
            }
            catch
            {
                // Any error during update check - just continue with current version
                updateForm?.Close();
                return false;
            }
        }

        private static void ApplyUpdateAndRestart(string currentExe, string newExe)
        {
            var batchPath = Path.Combine(Path.GetTempPath(), $"swgs-update-{Guid.NewGuid():N}.bat");

            // Create batch script to:
            // 1. Wait for current process to exit
            // 2. Delete old launcher
            // 3. Rename .new to original name
            // 4. Start updated launcher
            // 5. Delete itself
            var batchContent = $@"@echo off
timeout /t 2 /nobreak >nul
del ""{currentExe}""
move /y ""{newExe}"" ""{currentExe}""
start """" ""{currentExe}""
del ""%~f0""
";

            File.WriteAllText(batchPath, batchContent);

            // Launch batch script hidden
            var startInfo = new ProcessStartInfo
            {
                FileName = batchPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(startInfo);

            // Exit current application
            Application.Exit();
        }
    }
}
