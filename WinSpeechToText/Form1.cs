using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Icon = System.Drawing.Icon;
using SystemIcons = System.Drawing.SystemIcons; // Alias for System.Drawing.SystemIcons

namespace WinSpeechToText
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID_RECORD = 1;          // Original hotkey ID
        private const int HOTKEY_ID_TRANSLATE = 2;       // New hotkey ID for translation
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;         // Ctrl modifier
        private const uint VK_Q = 0x51;  // Q key
        private const uint VK_E = 0x45;  // E key

        private WaveInEvent waveSource;
        private WaveFileWriter waveFile;
        private string audioFilePath = "recorded_audio.wav";
        private bool isRecording = false;
        private bool isTranslating = false;
        private bool isTranslationMode = false;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private readonly string _apiKeyFilePath;
        private static readonly byte[] _entropy = null; // Optional entropy for DPAPI

        public Form1()
        {
            // Define path for API key storage in Local AppData
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataFolder, "WinSpeechToText");
            Directory.CreateDirectory(appFolder); // Ensure the directory exists
            _apiKeyFilePath = Path.Combine(appFolder, "apikey.dat");

            InitializeComponent();
            RegisterHotKey(this.Handle, HOTKEY_ID_RECORD, MOD_ALT, VK_Q);
            RegisterHotKey(this.Handle, HOTKEY_ID_TRANSLATE, MOD_ALT, VK_E);
            this.FormClosing += Form1_FormClosing;

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Exit", null, OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "WinSpeechToText";

            // Ensure the icon file path is correct
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon_win_speech_to_text.ico");
            if (File.Exists(iconPath))
            {
                trayIcon.Icon = new Icon(iconPath);
            }
            else
            {
                trayIcon.Icon = SystemIcons.Application;
            }

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
            
            // Check if application was started from Windows startup
            if (IsStartedFromWindowsStartup())
            {
                // Use BeginInvoke to minimize after the form is fully loaded
                this.BeginInvoke(new Action(() => MinimizeToTray()));                
            }
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();

                if (hotkeyId == HOTKEY_ID_RECORD || hotkeyId == HOTKEY_ID_TRANSLATE)
                {
                    if (this.WindowState == FormWindowState.Minimized || !this.Visible)
                    {
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                    }
                    this.Activate();

                    if (isRecording)
                    {
                        bool wasTranslationMode = isTranslationMode;
                        StopRecording();
                        isTranslationMode = wasTranslationMode; // Preserve mode
                    }
                    else
                    {
                        isTranslationMode = (hotkeyId == HOTKEY_ID_TRANSLATE);

                        if (isTranslationMode)
                        {
                            isTranslating = true;
                            lblStatus.Text = "Starting translation recording (press Alt+E to stop)...";
                        }
                        else
                        {
                            isTranslating = false;
                            lblStatus.Text = "Starting transcription recording (press Alt+Q to stop)...";
                        }

                        StartRecording();
                    }
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                CleanupResources();
                return;
            }

            // Hide to tray on normal close
            e.Cancel = true;
            this.Hide();

            if (isRecording)
            {
                StopRecording();
            }
        }

        private void CleanupResources()
        {
            if (isRecording)
            {
                try { StopRecording(); } catch { /* Ignore errors during shutdown */ }
            }

            try
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID_RECORD);
                UnregisterHotKey(this.Handle, HOTKEY_ID_TRANSLATE);
            }
            catch { /* Ignore errors during shutdown */ }

            // Dispose of notification icon
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            // Dispose any remaining resources
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            CleanupResources();
            Application.Exit();
        }

        private void StartRecording()
        {
            try
            {
                waveSource = new WaveInEvent();
                waveSource.WaveFormat = new WaveFormat(16000, 1); // 16kHz, Mono
                waveSource.DataAvailable += OnDataAvailable;
                waveSource.RecordingStopped += OnRecordingStopped;
                waveFile = new WaveFileWriter(audioFilePath, waveSource.WaveFormat);
                waveSource.StartRecording();
                isRecording = true;

                // Update UI
                if (isTranslationMode)
                    lblStatus.Text = "Translation recording in progress (press Alt+E to stop)...";
                else
                    lblStatus.Text = "Recording in progress (press Alt+Q to stop)...";

                txtTranscription.Clear();
                btnRecord.Text = "Stop";
                btnTranslate.Enabled = false;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error starting recording: " + ex.Message;
            }
        }

        private void StopRecording()
        {
            if (waveSource != null)
            {
                waveSource.StopRecording();
                isRecording = false;
                lblStatus.Text = "Processing audio...";
                btnRecord.Text = "Start";
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }

            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            // Update UI to show we're processing
            UpdateUiForProcessing(true);

            if (isTranslationMode)
                lblStatus.Text = "Sending audio for translation...";
            else
                lblStatus.Text = "Sending audio for transcription...";

            // Use BeginInvoke to ensure UI updates are processed before potentially blocking API calls
            if (isTranslating)
            {
                BeginInvoke(new Action(() => SendAudioToOpenAIForTranslation()));
                isTranslating = false;
            }
            else
            {
                BeginInvoke(new Action(() => SendAudioToOpenAI()));
            }
        }

        private async void SendAudioToOpenAI()
        {
            string apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                txtTranscription.Text = "Error: API key is not provided. Please set your OpenAI API key.";
                lblStatus.Text = "Error: API key missing";
                UpdateUiForProcessing(false);
                return;
            }

            string endpoint = "https://api.openai.com/v1/audio/transcriptions";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StringContent("gpt-4o-transcribe"), "model");
                        content.Add(new ByteArrayContent(File.ReadAllBytes(audioFilePath)), "file", "audio.wav");

                        HttpResponseMessage response = await client.PostAsync(endpoint, content);
                        string responseString = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonConvert.DeserializeObject<dynamic>(responseString);
                            string transcribedText = result["text"];

                            // Update the text box with the transcription
                            txtTranscription.Text = transcribedText;

                            // Automatically copy the transcribed text to clipboard
                            if (!string.IsNullOrEmpty(transcribedText))
                            {
                                Clipboard.SetText(transcribedText);
                                lblStatus.Text = "Transcription complete and copied to clipboard";
                            }
                            else
                            {
                                lblStatus.Text = "Transcription complete (empty result)";
                            }
                        }
                        else
                        {
                            txtTranscription.Text = $"API Error: {response.StatusCode}\r\n{responseString}";
                            lblStatus.Text = "Error in transcription";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtTranscription.Text = $"Error: {ex.Message}";
                lblStatus.Text = "Error in processing";
            }
            finally
            {
                UpdateUiForProcessing(false);
            }
        }

        private async void SendAudioToOpenAIForTranslation()
        {
            string apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                txtTranscription.Text = "Error: API key is not provided. Please set your OpenAI API key.";
                lblStatus.Text = "Error: API key missing";
                UpdateUiForProcessing(false);
                return;
            }

            string endpoint = "https://api.openai.com/v1/audio/translations";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StringContent("whisper-1"), "model");
                        content.Add(new ByteArrayContent(File.ReadAllBytes(audioFilePath)), "file", "audio.wav");

                        HttpResponseMessage response = await client.PostAsync(endpoint, content);
                        string responseString = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonConvert.DeserializeObject<dynamic>(responseString);
                            string translatedText = result["text"];

                            // Update the text box with the translation
                            txtTranscription.Text = translatedText;

                            // Automatically copy the translated text to clipboard
                            if (!string.IsNullOrEmpty(translatedText))
                            {
                                Clipboard.SetText(translatedText);
                                lblStatus.Text = "Translation complete and copied to clipboard";
                            }
                            else
                            {
                                lblStatus.Text = "Translation complete (empty result)";
                            }
                        }
                        else
                        {
                            txtTranscription.Text = $"API Error: {response.StatusCode}\r\n{responseString}";
                            lblStatus.Text = "Error in translation";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtTranscription.Text = $"Error: {ex.Message}";
                lblStatus.Text = "Error in processing";
            }
            finally
            {
                UpdateUiForProcessing(false);
            }
        }

        private void UpdateUiForProcessing(bool isProcessing)
        {
            btnRecord.Enabled = !isProcessing;
            btnTranslate.Enabled = !isProcessing;
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        private void btnTranslate_Click(object sender, EventArgs e)
        {
            isTranslating = true;
            isTranslationMode = true;  // Set the translation mode
            StartRecording();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtTranscription.Text);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            string existingApiKey = GetApiKey();
            using (var settingsDialog = new SettingsDialog(existingApiKey))
            {
                if (settingsDialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(settingsDialog.ApiKey))
                {
                    SaveApiKey(settingsDialog.ApiKey);
                }
            }
        }

        private void SaveApiKey(string apiKey)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey))
                {
                    if (File.Exists(_apiKeyFilePath))
                    {
                        File.Delete(_apiKeyFilePath); // Delete file if key is cleared
                    }
                    return;
                }

                byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);
                byte[] encryptedData = ProtectedData.Protect(apiKeyBytes, _entropy, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(_apiKeyFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving API key: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetApiKey()
        {
            try
            {
                if (!File.Exists(_apiKeyFilePath))
                {
                    return string.Empty;
                }

                byte[] encryptedData = File.ReadAllBytes(_apiKeyFilePath);
                byte[] apiKeyBytes = ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(apiKeyBytes);
            }
            catch (CryptographicException)
            {
                // Decryption failed (e.g., file corrupted or from different user/machine)
                try { File.Delete(_apiKeyFilePath); } catch { /* Ignore delete error */ }
                return string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading API key: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                MinimizeToTray();
                e.Handled = true;
            }
        }

        private void MinimizeToTray()
        {
            this.Hide();
            // Recording continues while minimized
            trayIcon.ShowBalloonTip(2000, "WinSpeechToText", "Application is running in the system tray", ToolTipIcon.Info);
        }
        
        /// <summary>
        /// Determines if the application was started from Windows startup.
        /// </summary>
        /// <returns>True if started from Windows startup, false otherwise.</returns>
        private bool IsStartedFromWindowsStartup()
        {
            try
            {
                // Get the startup registry key
                const string startupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                const string applicationName = "WinSpeechToText";
                
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(startupRegistryKey))
                {
                    if (key == null) return false;
                    
                    // Check if our application is in the startup registry
                    object value = key.GetValue(applicationName);
                    if (value == null) return false;
                    
                    // Get the command line arguments
                    string[] args = Environment.GetCommandLineArgs();
                    string exePath = Application.ExecutablePath;
                    
                    // Additional check: compare process start time with system uptime
                    // If the app was started very close to system startup, it's likely from Windows startup
                    TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                    DateTime processStartTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
                    TimeSpan timeSinceSystemStart = DateTime.Now - processStartTime;
                    
                    // If the process started within 2 minutes of system startup, consider it started from Windows startup
                    if (timeSinceSystemStart.TotalMinutes < 2)
                    {
                        return true;
                    }
                    
                    // Check for any startup-specific command line arguments if you have them
                    // For now, just check if the registry entry exists and the app started near system boot
                    return true;
                }
            }
            catch (Exception)
            {
                // If there's any error, assume it wasn't started from startup
                return false;
            }
        }
    }
}
