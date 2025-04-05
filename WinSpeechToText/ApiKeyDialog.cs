using System;
using System.ComponentModel; // Added for DesignerSerializationVisibility
using System.Windows.Forms;

namespace WinSpeechToText
{
    public partial class ApiKeyDialog : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ApiKey { get; private set; }

        public ApiKeyDialog(string existingApiKey = "")
        {
            InitializeComponent();
            txtApiKey.Text = existingApiKey;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ApiKey = txtApiKey.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
