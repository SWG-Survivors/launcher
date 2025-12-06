using System;
using System.Drawing;
using System.Windows.Forms;

namespace SWGSurvivors_Patcher
{
    public partial class UpdateForm : Form
    {
        private Label statusLabel = null!;
        private ProgressBar progressBar = null!;
        private Label versionLabel = null!;

        public UpdateForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "SWGSurvivors Launcher Update";
            this.ClientSize = new Size(500, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ControlBox = true; // Allow close button to terminate if stuck

            int yPos = 20;
            int margin = 20;
            int controlWidth = this.ClientSize.Width - (margin * 2);

            // Title
            Label titleLabel = new Label
            {
                Text = "Launcher Update",
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(margin, yPos)
            };
            this.Controls.Add(titleLabel);
            yPos += 40;

            // Version info label
            versionLabel = new Label
            {
                Text = "Checking for updates...",
                Font = new Font("Arial", 10),
                AutoSize = false,
                Size = new Size(controlWidth, 20),
                Location = new Point(margin, yPos)
            };
            this.Controls.Add(versionLabel);
            yPos += 30;

            // Status label
            statusLabel = new Label
            {
                Text = "Connecting to GitHub...",
                Font = new Font("Arial", 9),
                AutoSize = false,
                Size = new Size(controlWidth, 20),
                Location = new Point(margin, yPos),
                ForeColor = Color.DarkGray
            };
            this.Controls.Add(statusLabel);
            yPos += 30;

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(margin, yPos),
                Size = new Size(controlWidth, 25),
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            this.Controls.Add(progressBar);
        }

        public void SetStatus(string message)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action(() => statusLabel.Text = message));
            }
            else
            {
                statusLabel.Text = message;
            }
            Application.DoEvents();
        }

        public void SetVersionInfo(string message)
        {
            if (versionLabel.InvokeRequired)
            {
                versionLabel.Invoke(new Action(() => versionLabel.Text = message));
            }
            else
            {
                versionLabel.Text = message;
            }
            Application.DoEvents();
        }

        public void SetProgress(int percentage)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => progressBar.Value = Math.Min(Math.Max(percentage, 0), 100)));
            }
            else
            {
                progressBar.Value = Math.Min(Math.Max(percentage, 0), 100);
            }
            Application.DoEvents();
        }

        public void SetIndeterminate(bool indeterminate)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() =>
                    progressBar.Style = indeterminate ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous));
            }
            else
            {
                progressBar.Style = indeterminate ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            }
            Application.DoEvents();
        }
    }
}
