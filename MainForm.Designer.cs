using System.Net;
using Image = System.Drawing.Image;
using Timer = System.Timers.Timer;
using Newtonsoft.Json;
using System.Runtime.InteropServices;


namespace pakar_alert_overlay
{
    partial class MainForm
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);
        private System.ComponentModel.IContainer components = null;
        // context menu
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private ToolStrip exitMenuItem;
        // alerts
        private Image alertBackground = Properties.Resources.alert;
        private Image selectedAlertBackground = Properties.Resources.alert_selected;
        private FlowLayoutPanel alertsPanel;
        private AlertHeader Header;
        // minimize button
        private Button minimizeButton;
        // timers
        private Timer checkMessagesTimer;
        private Timer hideFormTimer;
        // logical variables
        private string[] alertedAreas = { };
        private string[] suppressedAreas = { };
        // behaivor variables
        public new bool ShowWithoutActivation = false;
        private bool isShowing;
        // settings page form
        private SettingsPage settingsPage;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new NotifyIcon(this.components);
            this.contextMenu = new ContextMenuStrip();
            this.exitMenuItem = new ToolStrip();
            this.checkMessagesTimer = new System.Timers.Timer();
            this.hideFormTimer = new System.Timers.Timer();
            this.Header = new AlertHeader();
            this.alertsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.settingsPage = new SettingsPage();
            this.minimizeButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.checkMessagesTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hideFormTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Header)).BeginInit();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)Properties.Resources.logo_alerts_desktop);
            this.notifyIcon.Visible = true;
            this.notifyIcon.Click += new System.EventHandler(this.notifyIconClick);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.Add("Exit", null, ExitMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // checkMessagesTimer
            // 
            this.checkMessagesTimer.Enabled = true;
            this.checkMessagesTimer.Interval = 2000D;
            this.checkMessagesTimer.SynchronizingObject = this;
            this.checkMessagesTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.CheckMessagesTimer_Tick);
            // 
            // hideFormTimer
            // 
            this.hideFormTimer.AutoReset = false;
            this.hideFormTimer.Enabled = true;
            this.hideFormTimer.Interval = 10000D;
            this.hideFormTimer.SynchronizingObject = this;
            this.hideFormTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.hideForm);
            this.hideFormTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.clearSuppressed);

            // 
            // Header
            // 
            this.Header.Image = Properties.Resources.banner;
            this.Header.InitialImage = Properties.Resources.banner;
            this.Header.Location = new System.Drawing.Point(0, 0);
            this.Header.Name = "Header";
            this.Header.Size = new System.Drawing.Size(400, 80);
            this.Header.TabIndex = 0;
            this.Header.TabStop = false;
            // 
            // mainFlowLayoutPanel
            // 
            this.alertsPanel.AutoSize = true;
            this.alertsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.alertsPanel.Location = new System.Drawing.Point(0, 80);
            this.alertsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.alertsPanel.Name = "mainFlowLayoutPanel";
            this.alertsPanel.Size = new System.Drawing.Size(400, 220);
            this.alertsPanel.TabIndex = 1;
            // 
            // settingsPage
            // 
            this.settingsPage.Size = new System.Drawing.Size(274, 485);
            this.settingsPage.Location = new System.Drawing.Point(200, 200);
            this.settingsPage.Margin = new System.Windows.Forms.Padding(2);
            this.settingsPage.Name = "settingsPage";
            this.settingsPage.Text = "Settings";
            this.settingsPage.Visible = false;
            // 
            // minimizeButton
            // 
            this.minimizeButton.BackColor = System.Drawing.Color.Transparent;
            this.minimizeButton.Location = new System.Drawing.Point(0, 0);
            this.minimizeButton.Name = "minimizeButton";
            this.minimizeButton.Size = new System.Drawing.Size(20, 20);
            this.minimizeButton.TabIndex = 2;
            this.minimizeButton.Text = "-";
            this.minimizeButton.UseVisualStyleBackColor = false;
            this.minimizeButton.Click += new System.EventHandler(this.minimizeIconClick);
            // 
            // MainForm
            // 
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Magenta;
            this.Controls.Add(this.minimizeButton);
            this.Controls.Add(this.alertsPanel);
            this.Controls.Add(this.Header);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(50, 50);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Magenta;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.hideForm);
            ((System.ComponentModel.ISupportInitialize)(this.checkMessagesTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hideFormTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Header)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int formWidth = this.Width;
            int formHeight = this.Height;

            // Calculate the new X and Y coordinates for the form
            int newX = screenWidth - formWidth - 50; // 50 pixels from the right
            int newY = 50; // 50 pixels from the top

            // Set the form's new location
            this.Location = new Point(newX, newY);
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            // Clean up and exit the application
            notifyIcon.Visible = false;
            Environment.Exit(0);
        }

        private void hideForm(object sender, EventArgs e)
        {
            Logger.Log(LogLevel.Info, "hideForm");
            Invoke(new Action(this.Hide));
            this.isShowing = false;
        }

        private void notifyIconClick(object sender, EventArgs e)
        {
            Logger.Log(LogLevel.Info, "notifyIconClick");
            if (this.settingsPage.settings.canOpenSettings)
            {
                this.settingsPage.Show();
            }
            else
            {
                Logger.Log(LogLevel.Warning, "User is not allowed to edit settings");
            }
        }

        private void minimizeIconClick(object sender, EventArgs e)
        {
            this.hideForm(sender, e);
            // add current alerts to suppressedAreas
            this.suppressedAreas = this.suppressedAreas.Concat(this.alertedAreas).ToArray();
            // keep distinct values
            this.suppressedAreas = this.suppressedAreas.Distinct().ToArray();
            Logger.Log(LogLevel.Info, $"suppressing alerts for areas [{String.Join(",", this.suppressedAreas)}]");
        }

        private void ShowForm()
        {
            Logger.Log(LogLevel.Info, "ShowForm");
            this.Show();
            this.DisplayAlertBanners();
            this.isShowing = true;
        }

        private void ResetHideFormTimer()
        {
            this.hideFormTimer.Stop();
            this.hideFormTimer.Start();
            Logger.Log(LogLevel.Info, "Reset the hideFormTimer");
        }

        private bool ShouldShowAlerts(AlertsJson alerts)
        {
            bool isNewAlert = !alerts.data.SequenceEqual(this.alertedAreas) || alerts.title != this.Header.text;
            bool allAlertsSuppressed = alerts.data.All(item => this.suppressedAreas.Contains(item));
            bool isRelevantAreaAlert = IsRelevantAreaAlert(alerts);

            return (isNewAlert || !this.isShowing) && !allAlertsSuppressed && isRelevantAreaAlert;
        }

        private bool IsRelevantAreaAlert(AlertsJson alerts)
        {
            bool alertOnlySelectedAreas = this.settingsPage.settings.AlertSelectedAreas;
            List<string> selectedAreas = this.settingsPage.settings.SelectedAreas;
            if (alertOnlySelectedAreas)
            {
                return alerts.data.Any(alert => selectedAreas.Contains(alert));
            }
            return true; // If alertOnlySelectedAreas is false, always return true.
        }

        private async void CheckMessagesTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                string alertsUrl = this.settingsPage.settings.AlertsUrl;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(alertsUrl);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Referer = "https://www.oref.org.il/";
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string responseBody = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();

                        if (responseBody.Trim().Length > 0)
                        {
                            AlertsJson responseData = JsonConvert.DeserializeObject<AlertsJson>(responseBody);
                            ShowAlerts(responseData);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error: " + ex.Message);
            }
        }

        private void ShowAlerts(AlertsJson alerts)
        {
            this.alertedAreas = alerts.data;
            this.Header.text = alerts.title;

            if (ShouldShowAlerts(alerts))
            {
                Logger.Log(LogLevel.Info, $"Received alert of type {alerts.title} for areas [{String.Join(",", alerts.data)}]");

                this.Header.Invalidate(); // redraw header
                WakeScreen();
                Invoke(new Action(this.ShowForm));

                if (this.settingsPage.settings.SoundAlarm)
                {
                    SoundAlarm(alerts.data);
                }
            }
            this.ResetHideFormTimer();
        }

        private void DisplayAlertBanners()
        {
            alertsPanel.Controls.Clear();
            List<string> selectedAreas = this.settingsPage.settings.SelectedAreas;
            string[] sortedAreas = alertedAreas
                .OrderByDescending(area => selectedAreas.Contains(area))
                .ToArray();

            foreach (string area in sortedAreas)
            {
                // Create PictureBox
                PictureBox pictureBox = new AlertBanner(area);
                pictureBox.Image = (selectedAreas.Contains(area)) ? selectedAlertBackground : alertBackground;
                pictureBox.Size = new Size(400, 40);
                pictureBox.Margin = new Padding(0, 2, 0, 0);

                pictureBox.Invalidate();

                alertsPanel.Controls.Add(pictureBox);
            }
        }

        private void SoundAlarm(string[] alertedAreas)
        {
            bool shouldSoundAlarm = false;
            List<string> selectedAreas = this.settingsPage.settings.SelectedAreas;
            foreach (string alertedArea in alertedAreas)
            {
                if (selectedAreas.Contains(alertedArea))
                {
                    shouldSoundAlarm = true;
                    break;
                }
            }
            if (shouldSoundAlarm)
            {
                Logger.Log(LogLevel.Info, $"Sounding Alarm!");
                MP3Player.SoundAlarm();
            }
        }

        private void clearSuppressed(object sender, EventArgs e)
        {
            Logger.Log(LogLevel.Info, "clearSuppressed");
            if (this.suppressedAreas.Length > 0)
            {
                Logger.Log(LogLevel.Info, "Wave is over, clearing suppressed alerts");
                this.suppressedAreas = new string[0];
            }
        }

        public static void WakeScreen()
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MONITORPOWER = 0xF170;
            const int MonitorTurnOn = -1;
            SendMessage(IntPtr.Zero, WM_SYSCOMMAND, SC_MONITORPOWER, MonitorTurnOn);
        }

    }
}

