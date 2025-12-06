using System;
using System.IO;
using System.Text.Json;

namespace SWGSurvivors_Patcher
{
    public class LauncherSettings
    {
        private const string SETTINGS_FILE = "SWGSurvivors-Launcher.cfg";
        private static readonly string settingsPath = Path.Combine(AppContext.BaseDirectory, SETTINGS_FILE);

        // Settings properties
        public bool PatchAutomaticallyOnStartup { get; set; } = false;

        /// <summary>
        /// Loads settings from the JSON config file. If the file doesn't exist, returns default settings.
        /// </summary>
        public static LauncherSettings Load()
        {
            try
            {
                if (!File.Exists(settingsPath))
                {
                    // File doesn't exist, return defaults
                    return new LauncherSettings();
                }

                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<LauncherSettings>(json);
                return settings ?? new LauncherSettings();
            }
            catch (Exception)
            {
                // If there's any error reading/parsing the file, return defaults
                return new LauncherSettings();
            }
        }

        /// <summary>
        /// Saves settings to the JSON config file.
        /// </summary>
        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a copy of the current settings object.
        /// </summary>
        public LauncherSettings Clone()
        {
            return new LauncherSettings
            {
                PatchAutomaticallyOnStartup = this.PatchAutomaticallyOnStartup
            };
        }
    }
}
