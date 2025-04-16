using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.ComponentModel;

namespace WinSpeechToText
{
    public partial class SettingsDialog : Form
    {
        private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string ApplicationName = "WinSpeechToText";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ApiKey { get; private set; }

        public SettingsDialog(string existingApiKey = "")
        {
            InitializeComponent();
            txtApiKey.Text = existingApiKey;
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Check if application is set to run at Windows startup
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey))
            {
                chkRunAtStartup.Checked = key?.GetValue(ApplicationName) != null;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SaveSettings()
        {
            try
            {
                // Save the Windows startup setting
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true))
                {
                    if (key != null)
                    {
                        if (chkRunAtStartup.Checked)
                        {
                            // Add application to startup
                            string appPath = Application.ExecutablePath;
                            key.SetValue(ApplicationName, appPath);
                        }
                        else
                        {
                            // Remove application from startup
                            if (key.GetValue(ApplicationName) != null)
                                key.DeleteValue(ApplicationName);
                        }
                    }
                }

                // Save the API key
                ApiKey = txtApiKey.Text.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}