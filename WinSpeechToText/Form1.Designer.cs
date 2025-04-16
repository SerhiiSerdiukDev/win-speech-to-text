namespace WinSpeechToText
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            btnRecord = new System.Windows.Forms.Button();
            btnTranslate = new System.Windows.Forms.Button();
            txtTranscription = new System.Windows.Forms.TextBox();
            lblStatus = new System.Windows.Forms.Label();
            btnCopy = new System.Windows.Forms.Button();
            btnSettings = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // btnRecord
            // 
            btnRecord.BackColor = System.Drawing.SystemColors.Control;
            btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnRecord.Location = new System.Drawing.Point(889, 16);
            btnRecord.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnRecord.Name = "btnRecord";
            btnRecord.Size = new System.Drawing.Size(160, 49);
            btnRecord.TabIndex = 0;
            btnRecord.Text = "To text";
            btnRecord.UseVisualStyleBackColor = false;
            btnRecord.Click += btnRecord_Click;
            // 
            // btnTranslate
            // 
            btnTranslate.Location = new System.Drawing.Point(889, 75);
            btnTranslate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnTranslate.Name = "btnTranslate";
            btnTranslate.Size = new System.Drawing.Size(160, 49);
            btnTranslate.TabIndex = 1;
            btnTranslate.Text = "To English";
            btnTranslate.UseVisualStyleBackColor = true;
            btnTranslate.Click += btnTranslate_Click;
            // 
            // txtTranscription
            // 
            txtTranscription.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtTranscription.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txtTranscription.Location = new System.Drawing.Point(16, 16);
            txtTranscription.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtTranscription.Multiline = true;
            txtTranscription.Name = "txtTranscription";
            txtTranscription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtTranscription.Size = new System.Drawing.Size(860, 224);
            txtTranscription.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(13, 246);
            lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(50, 20);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Ready";
            // 
            // btnCopy
            // 
            btnCopy.Location = new System.Drawing.Point(889, 134);
            btnCopy.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new System.Drawing.Size(160, 49);
            btnCopy.TabIndex = 5;
            btnCopy.Text = "Copy to Clipboard";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            // 
            // btnSettings
            // 
            btnSettings.Location = new System.Drawing.Point(889, 191);
            btnSettings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(160, 49);
            btnSettings.TabIndex = 6;
            btnSettings.Text = "Settings";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += btnSettings_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1062, 278);
            Controls.Add(btnSettings);
            Controls.Add(btnCopy);
            Controls.Add(lblStatus);
            Controls.Add(txtTranscription);
            Controls.Add(btnTranslate);
            Controls.Add(btnRecord);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "Form1";
            Text = "Win Speech To Text";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnTranslate;
        private System.Windows.Forms.TextBox txtTranscription;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnSettings;
    }
}

