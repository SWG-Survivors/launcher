using System;
using System.Drawing;
using System.Windows.Forms;

namespace SWGSurvivors_Patcher
{
    public partial class SettingsForm : Form
    {
        private CheckBox autoPatchCheckBox = null!;
        private Button okButton = null!;
        private Button cancelButton = null!;

        private LauncherSettings settings;
        private bool settingsChanged = false;

        public SettingsForm(LauncherSettings currentSettings)
        {
            settings = currentSettings.Clone(); // Work on a copy
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Launcher Settings";
            this.Size = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            int yPos = 20;

            // Title
            Label titleLabel = new Label
            {
                Text = "Launcher Settings",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(titleLabel);
            yPos += 40;

            // Auto-patch checkbox
            autoPatchCheckBox = new CheckBox
            {
                Text = "Automatically patch on startup",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(20, yPos),
                Checked = settings.PatchAutomaticallyOnStartup
            };
            autoPatchCheckBox.CheckedChanged += (s, e) => settingsChanged = true;
            this.Controls.Add(autoPatchCheckBox);
            yPos += 50;

            // Buttons
            int buttonWidth = 100;
            int buttonSpacing = 10;
            int totalWidth = (buttonWidth * 2) + buttonSpacing;
            int startX = (this.ClientSize.Width - totalWidth) / 2;

            okButton = new Button
            {
                Text = "OK",
                Size = new Size(buttonWidth, 35),
                Location = new Point(startX, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(buttonWidth, 35),
                Location = new Point(startX + buttonWidth + buttonSpacing, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(cancelButton);

            // Set accept and cancel buttons
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (settingsChanged)
            {
                // Update settings from UI
                settings.PatchAutomaticallyOnStartup = autoPatchCheckBox.Checked;

                try
                {
                    // Save settings to file
                    settings.Save();
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to save settings: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None; // Keep dialog open
                }
            }
            else
            {
                // No changes, just close
                this.DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Gets the updated settings after the dialog is closed with OK.
        /// </summary>
        public LauncherSettings GetSettings()
        {
            return settings;
        }
    }
}
