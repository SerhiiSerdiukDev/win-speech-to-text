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

        private const int HOTKEY_ID = 1;  // Unique ID
        private const uint MOD_CTRL = 0x0002; // Ctrl key
        private const uint MOD_ALT = 0x0001; // Alt key
        private const uint VK_SPACE = 0x20;  // Space key

        private WaveInEvent waveSource;
        private WaveFileWriter waveFile;
        private string audioFilePath = "recorded_audio.wav";
        private bool isRecording = false;
        private bool isTranslating = false;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public Form1()
        {
            InitializeComponent();
            // Register Ctrl+Alt+Space as the global hotkey
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CTRL | MOD_ALT, VK_SPACE);
            this.FormClosing += Form1_FormClosing;

            // Initialize Tray Icon
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "WinSpeechToText";
            trayIcon.Icon = new Icon("icon_win_speech_to_text.ico");
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

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
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if (this.WindowState == FormWindowState.Minimized || !this.Visible)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                }

                if (isRecording)
                    StopRecording();
                else
                    StartRecording();
            }
            base.WndProc(ref m);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            UnregisterHotKey(this.Handle, HOTKEY_ID);

            // Clean up recording resources if still active
            if (isRecording)
            {
                StopRecording();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            trayIcon.Visible = false;
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
                lblStatus.Text = "Recording in progress...";
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
            lblStatus.Text = "Sending audio to OpenAI...";

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
                        content.Add(new StringContent("whisper-1"), "model");
                        content.Add(new ByteArrayContent(File.ReadAllBytes(audioFilePath)), "file", "audio.wav");

                        HttpResponseMessage response = await client.PostAsync(endpoint, content);
                        string responseString = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonConvert.DeserializeObject<dynamic>(responseString);
                            string transcribedText = result["text"];

                            // Update the text box with the transcription
                            txtTranscription.Text = transcribedText;
                            lblStatus.Text = "Transcription complete";
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
                            lblStatus.Text = "Translation complete";
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
    }
}
