using Microsoft.Win32;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.CompilerServices;

namespace pakar_alert_overlay
{
    public class SettingsModel
    {
        private static readonly string defaultAreasUrl = "https://www.oref.org.il/Shared/Ajax/GetDistricts.aspx?lang=he";
        private static readonly string defaultAlertsUrl = "https://www.oref.org.il/WarningMessages/alert/alerts.json";
        private const string RegistryKey = "Software\\alert-overlay\\Settings";
        public List<string> SelectedAreas { get; set; }
        public bool StartWithComputer { get; set; }
        public bool SoundAlarm { get; set; }
        public bool AlertSelectedAreas { get; set; }
        public string AlertsUrl { get; set; } = defaultAlertsUrl;
        public string AreasUrl { get; set; } = defaultAreasUrl;
        public bool canOpenSettings { get; set; }

        public void LoadFromRegistry()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RegistryKey))
            {
                if (key != null)
                {
                    this.SelectedAreas = ((string[])key.GetValue("SelectedAreas", new string[0])).ToList<string>();
                    this.StartWithComputer = (string)key.GetValue("StartWithComputer", "False") == "True";
                    this.SoundAlarm = (string)key.GetValue("soundAlarm", "False") == "True";
                    this.AlertSelectedAreas = (string)key.GetValue("alertSelectedAreas", "False") == "True";
                    this.AlertsUrl = (string)key.GetValue("alertsUrl", defaultAlertsUrl);
                    this.AreasUrl = (string)key.GetValue("areasUrl", defaultAreasUrl);
                    this.canOpenSettings = (string)key.GetValue("canOpenSettings", "True") == "True";
                    key.Close();
                }
                else
                {
                    this.SelectedAreas = new List<string>();
                    this.StartWithComputer = false;
                    this.SoundAlarm = false;
                    this.AlertSelectedAreas = false;
                    this.AlertsUrl = defaultAlertsUrl;
                    this.AreasUrl = defaultAreasUrl;
                    this.canOpenSettings = true;
                }
            }
        }

        public void SaveToRegistry()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKey))
            {
                if (key != null)
                {
                    key.SetValue("SelectedAreas", this.SelectedAreas.ToArray());
                    key.SetValue("StartWithComputer", this.StartWithComputer);
                    key.SetValue("soundAlarm", this.SoundAlarm);
                    key.SetValue("alertSelectedAreas", this.AlertSelectedAreas);
                    key.SetValue("alertsUrl", this.AlertsUrl);
                    key.SetValue("areasUrl", this.AreasUrl);

                    key.Close();

                    // Set the application to start at logon if startWithComputerCheckBox is true
                    if (this.StartWithComputer)
                    {
                        // Specify the path to your application executable
                        string appPath = Application.ExecutablePath;

                        // Create a registry key in the "Run" registry location
                        using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                        {
                            if (runKey != null)
                            {
                                // Set the registry value to your application path
                                runKey.SetValue("alert-overlay", appPath);
                                runKey.Close();
                            }
                        }
                    }
                    else
                    {
                        // Remove the registry key if startWithComputerCheckBox is false
                        using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                        {
                            if (runKey != null)
                            {
                                // Remove the registry value for your application
                                runKey.DeleteValue("alert-overlay", false);
                                runKey.Close();
                            }
                        }
                    }
                }
            }
        }
    }
    public partial class SettingsPage : Form
    {
        public SettingsModel settings;
        private Label selectedAreasLabel;
        private CheckedListBox selectedAreasCheckedListBox;
        private CheckBox soundAlarmCheckBox;
        private CheckBox alertSelectedCheckBox;
        private CheckBox startWithComputerCheckBox;
        private Button saveButton;
        private Label alertsUrlLabel;
        public TextBox alertsUrlTextBox;
        private Label areasUrlLabel;
        private TextBox areasUrlTextBox;

        private bool isMouseClick = false;


        public SettingsPage()
        {
            this.Visible = false;
            this.Icon = Properties.Resources.logo_alerts_desktop;

            this.selectedAreasLabel = new Label();
            this.selectedAreasCheckedListBox = new CheckedListBox();
            this.alertsUrlLabel = new Label();
            this.alertsUrlTextBox = new TextBox();
            this.areasUrlLabel = new Label();
            this.areasUrlTextBox = new TextBox();
            this.soundAlarmCheckBox = new CheckBox();
            this.alertSelectedCheckBox = new CheckBox();
            this.startWithComputerCheckBox = new CheckBox();
            this.saveButton = new Button();


            InitializeComponent();

            // Initialize the settings object
            this.settings = new SettingsModel();
            this.settings.LoadFromRegistry();

            string[] availableAlarmAreas = GetAllAreas();

            // Bind controls to settings
            this.selectedAreasCheckedListBox.DataSource = availableAlarmAreas;
            this.startWithComputerCheckBox.DataBindings.Add("Checked", settings, "StartWithComputer");
            this.soundAlarmCheckBox.DataBindings.Add("Checked", settings, "SoundAlarm");
            this.alertSelectedCheckBox.DataBindings.Add("Checked", settings, "AlertSelectedAreas");
            this.alertsUrlTextBox.DataBindings.Add("Text", settings, "AlertsUrl");
            this.areasUrlTextBox.DataBindings.Add("Text", settings, "AreasUrl");

            // Set up initial selections
            if (this.settings.SelectedAreas.Count > 0)
                selectedAreasCheckedListBox.SelectedItem = this.settings.SelectedAreas[0];
            foreach (var area in this.settings.SelectedAreas)
            {
                int index = selectedAreasCheckedListBox.Items.IndexOf(area);
                if (index != -1)
                    selectedAreasCheckedListBox.SetItemChecked(index, true);
            }
            this.selectedAreasCheckedListBox.ItemCheck += selectedAreasCheckedListBox_ItemCheck;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // alarmAreasLabel
            // 
            this.selectedAreasLabel.AutoSize = true;
            this.selectedAreasLabel.Location = new System.Drawing.Point(8, 8);
            this.selectedAreasLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.selectedAreasLabel.Name = "selectedAreasLabel";
            this.selectedAreasLabel.Size = new System.Drawing.Size(66, 13);
            this.selectedAreasLabel.TabIndex = 0;
            this.selectedAreasLabel.Text = "Selected Areas:";
            // 
            // alarmAreasListBox
            // 
            this.selectedAreasCheckedListBox.FormattingEnabled = true;
            this.selectedAreasCheckedListBox.Location = new System.Drawing.Point(8, 24);
            this.selectedAreasCheckedListBox.Margin = new System.Windows.Forms.Padding(2);
            this.selectedAreasCheckedListBox.Name = "selectedAreasCheckedListBox";
            this.selectedAreasCheckedListBox.Size = new System.Drawing.Size(206, 160);
            this.selectedAreasCheckedListBox.CheckOnClick = false;
            this.selectedAreasCheckedListBox.MouseClick += selectedAreasCheckedListBox_MouseUp;
            this.selectedAreasCheckedListBox.TabIndex = 1;
            // 
            // alertsUrlLabel
            // 
            this.alertsUrlLabel.AutoSize = true;
            this.alertsUrlLabel.Location = new System.Drawing.Point(8, 190);
            this.alertsUrlLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.alertsUrlLabel.Name = "alertsUrlLabel";
            this.alertsUrlLabel.Size = new System.Drawing.Size(61, 13);
            this.alertsUrlLabel.TabIndex = 2;
            this.alertsUrlLabel.Text = "Alerts URL:";
            // 
            // alertsUrlTextBox
            // 
            this.alertsUrlTextBox.Location = new System.Drawing.Point(8, 206);
            this.alertsUrlTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.alertsUrlTextBox.Name = "alertsUrlTextBox";
            this.alertsUrlTextBox.Size = new System.Drawing.Size(206, 20);
            this.alertsUrlTextBox.TabIndex = 3;
            // 
            // areasUrlLabel
            // 
            this.areasUrlLabel.AutoSize = true;
            this.areasUrlLabel.Location = new System.Drawing.Point(8, 236);
            this.areasUrlLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.areasUrlLabel.Name = "areasUrlLabel";
            this.areasUrlLabel.Size = new System.Drawing.Size(62, 13);
            this.areasUrlLabel.TabIndex = 4;
            this.areasUrlLabel.Text = "Areas URL:";
            // 
            // areasUrlTextBox
            // 
            this.areasUrlTextBox.Location = new System.Drawing.Point(8, 252);
            this.areasUrlTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.areasUrlTextBox.Name = "areasUrlTextBox";
            this.areasUrlTextBox.Size = new System.Drawing.Size(206, 20);
            this.areasUrlTextBox.TabIndex = 5;
            // 
            // soundAlarmCheckBox
            // 
            this.soundAlarmCheckBox.AutoSize = true;
            this.soundAlarmCheckBox.Location = new System.Drawing.Point(8, 278);
            this.soundAlarmCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.soundAlarmCheckBox.Name = "soundAlarmCheckBox";
            this.soundAlarmCheckBox.Size = new System.Drawing.Size(168, 17);
            this.soundAlarmCheckBox.TabIndex = 6;
            this.soundAlarmCheckBox.Text = "Sound alarm on selected areas";
            // 
            // alertSelectedCheckBox
            // 
            this.alertSelectedCheckBox.AutoSize = true;
            this.alertSelectedCheckBox.Location = new System.Drawing.Point(8, 304);
            this.alertSelectedCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.alertSelectedCheckBox.Name = "alertSelectedCheckBox";
            this.alertSelectedCheckBox.Size = new System.Drawing.Size(168, 17);
            this.alertSelectedCheckBox.TabIndex = 7;
            this.alertSelectedCheckBox.Text = "Alert only on selected areas";
            // 
            // startWithComputerCheckBox
            // 
            this.startWithComputerCheckBox.AutoSize = true;
            this.startWithComputerCheckBox.Location = new System.Drawing.Point(8, 330);
            this.startWithComputerCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.startWithComputerCheckBox.Name = "startWithComputerCheckBox";
            this.startWithComputerCheckBox.Size = new System.Drawing.Size(118, 17);
            this.startWithComputerCheckBox.TabIndex = 8;
            this.startWithComputerCheckBox.Text = "Start with Computer";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(8, 356);
            this.saveButton.Margin = new System.Windows.Forms.Padding(2);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(56, 20);
            this.saveButton.TabIndex = 9;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += this.saveButton_Click;
            // 
            // SettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.soundAlarmCheckBox);
            this.Controls.Add(this.startWithComputerCheckBox);
            this.Controls.Add(this.areasUrlTextBox);
            this.Controls.Add(this.areasUrlLabel);
            this.Controls.Add(this.alertsUrlTextBox);
            this.Controls.Add(this.alertsUrlLabel);
            this.Controls.Add(this.selectedAreasCheckedListBox);
            this.Controls.Add(this.alertSelectedCheckBox);
            this.Controls.Add(this.selectedAreasLabel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SettingsPage";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsPage_Close);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void SettingsPage_Close(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            this.settings.SaveToRegistry();
            MessageBox.Show("Settings saved successfully.");
        }

        private void selectedAreasCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string checkedItem = this.selectedAreasCheckedListBox.Items[e.Index].ToString();

            if (e.NewValue == CheckState.Checked && !settings.SelectedAreas.Contains(checkedItem))
            {
                settings.SelectedAreas.Add(checkedItem);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                settings.SelectedAreas.Remove(checkedItem);
            }

            if (!isMouseClick)
            {
                // Prevent the check state from changing if not caused by a mouse click
                e.NewValue = e.CurrentValue;
            }
        }

        private void selectedAreasCheckedListBox_MouseUp(object sender, MouseEventArgs e)
        {
            CheckedListBox checkedListBox = sender as CheckedListBox;
            if (checkedListBox != null)
            {
                // Determine the index of the item that was clicked.
                int index = checkedListBox.IndexFromPoint(e.Location);
                if (index != CheckedListBox.NoMatches)
                {
                    isMouseClick = true; // Set the flag before changing the check state
                    bool isChecked = checkedListBox.GetItemChecked(index);
                    checkedListBox.SetItemChecked(index, !isChecked);
                    isMouseClick = false; // Reset the flag after changing the check state
                }
            }
        }

        private string[] GetAllAreas()
        {
            string areasUrl = this.settings.AreasUrl;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(areasUrl);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Referer = "https://www.oref.org.il/";
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Check if there's a specific message
                    string responseBody = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();
                    AreaJson[] responseData = JsonConvert.DeserializeObject<AreaJson[]>(responseBody);
                    string[] labelArray = responseData.Select(area => area.label).Distinct().ToArray();
                    Array.Sort(labelArray);
                    return labelArray;
                }
                else { return new string[0]; }
            }
        }
    }
}
