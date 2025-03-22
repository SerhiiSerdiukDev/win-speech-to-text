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
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.txtTranscription = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSetApiKey = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRecord
            // 
            this.btnRecord.BackColor = System.Drawing.SystemColors.Control;
            this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnRecord.Location = new System.Drawing.Point(889, 13);
            this.btnRecord.Margin = new System.Windows.Forms.Padding(4);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(160, 39);
            this.btnRecord.TabIndex = 0;
            this.btnRecord.Text = "To text";
            this.btnRecord.UseVisualStyleBackColor = false;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // btnTranslate
            // 
            this.btnTranslate.Location = new System.Drawing.Point(889, 60);
            this.btnTranslate.Margin = new System.Windows.Forms.Padding(4);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(160, 39);
            this.btnTranslate.TabIndex = 1;
            this.btnTranslate.Text = "To English";
            this.btnTranslate.UseVisualStyleBackColor = true;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // txtTranscription
            // 
            this.txtTranscription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTranscription.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTranscription.Location = new System.Drawing.Point(16, 13);
            this.txtTranscription.Margin = new System.Windows.Forms.Padding(4);
            this.txtTranscription.Multiline = true;
            this.txtTranscription.Name = "txtTranscription";
            this.txtTranscription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTranscription.Size = new System.Drawing.Size(860, 180);
            this.txtTranscription.TabIndex = 2;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(13, 197);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(48, 16);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Ready";
            // 
            // btnSetApiKey
            // 
            this.btnSetApiKey.Location = new System.Drawing.Point(889, 154);
            this.btnSetApiKey.Margin = new System.Windows.Forms.Padding(4);
            this.btnSetApiKey.Name = "btnSetApiKey";
            this.btnSetApiKey.Size = new System.Drawing.Size(160, 39);
            this.btnSetApiKey.TabIndex = 4;
            this.btnSetApiKey.Text = "Set API Key";
            this.btnSetApiKey.UseVisualStyleBackColor = true;
            this.btnSetApiKey.Click += new System.EventHandler(this.btnSetApiKey_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(889, 107);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(160, 39);
            this.btnCopy.TabIndex = 5;
            this.btnCopy.Text = "Copy to Clipboard";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 222);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnSetApiKey);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtTranscription);
            this.Controls.Add(this.btnTranslate);
            this.Controls.Add(this.btnRecord);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Win Speech To Text";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnTranslate;
        private System.Windows.Forms.TextBox txtTranscription;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSetApiKey;
        private System.Windows.Forms.Button btnCopy;
    }
}

