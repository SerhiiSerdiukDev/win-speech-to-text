using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;
using Newtonsoft.Json;
using CredentialManagement;

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
        private ContextMenu trayMenu;

        public Form1()
        {
            InitializeComponent();
            // Register both hotkeys
            RegisterHotKey(this.Handle, HOTKEY_ID_RECORD, MOD_ALT, VK_Q);
            RegisterHotKey(this.Handle, HOTKEY_ID_TRANSLATE, MOD_ALT, VK_E); // Changed from MOD_CONTROL to MOD_ALT
            this.FormClosing += Form1_FormClosing;
            
            // Enable handling of keyboard input
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // Initialize Tray Icon
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

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

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
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
                        // When stopping recording, preserve the current mode
                        // This ensures that Alt+Q stops transcription and Alt+E stops translation
                        bool wasTranslationMode = isTranslationMode;
                        StopRecording();
                        isTranslationMode = wasTranslationMode; // Preserve mode during processing
                    }
                    else
                    {
                        // Set the translation mode based on which hotkey was pressed
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
            // Check if Windows is shutting down - don't prevent this
            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                // Clean up resources before shutdown
                CleanupResources();
                return; // Allow the form to close
            }

            // For normal closing (user clicked X), just hide to tray
            e.Cancel = true;
            this.Hide();

            if (isRecording)
            {
                StopRecording();
            }
        }

        // Add a new method to properly clean up resources
        private void CleanupResources()
        {
            // Make sure recording is stopped
            if (isRecording)
            {
                try
                {
                    // Use try-catch as we're in shutdown and want to ensure everything gets cleaned up
                    StopRecording();
                }
                catch { /* Ignore errors during shutdown */ }
            }

            // Unregister hotkeys
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
                    lblStatus.Text = "Translation recording in progress (press Alt+E to stop)..."; // Changed from Ctrl+E to Alt+E
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

            // Use BeginInvoke to ensure UI updates are processed
            if (isTranslating)
            {
                BeginInvoke(new Action(() => SendAudioToOpenAIForTranslation()));
                isTranslating = false;
            }
            else
            {
                BeginInvoke(new Action(() => SendAudioToOpenAI()));
            }
            
            // Don't reset the translation mode flag here anymore
            // The mode is now preserved by the WndProc method
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

        private void btnSetApiKey_Click(object sender, EventArgs e)
        {
            string existingApiKey = GetApiKey();
            using (var dialog = new ApiKeyDialog(existingApiKey))
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SaveApiKey(dialog.ApiKey);
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtTranscription.Text);
        }

        private void SaveApiKey(string apiKey)
        {
            var credential = new Credential
            {
                Target = "WinSpeechToTextApiKey",
                Username = "APIKey",
                Password = apiKey,
                PersistanceType = PersistanceType.LocalComputer
            };
            credential.Save();
        }

        private string GetApiKey()
        {
            var credential = new Credential { Target = "WinSpeechToTextApiKey" };
            if (credential.Load())
            {
                return credential.Password;
            }
            return string.Empty;
        }

        // Add KeyDown event handler for the Escape key
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // Minimize to system tray when Escape is pressed
                MinimizeToTray();
                e.Handled = true;  // Prevent further processing of this key
            }
        }

        // Add a helper method to minimize to tray
        private void MinimizeToTray()
        {
            // Hide the form
            this.Hide();
            
            // If recording is in progress, don't stop it
            // This allows the user to continue recording even when minimized
            
            // Show a notification if needed
            // trayIcon.ShowBalloonTip(1000, "WinSpeechToText", "Application minimized to tray", ToolTipIcon.Info);
        }
    }
}
