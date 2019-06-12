﻿namespace PlenBotLogUploader
{
    partial class FormLogSession
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
            this.buttonSessionStarter = new System.Windows.Forms.Button();
            this.checkBoxSupressWebhooks = new System.Windows.Forms.CheckBox();
            this.checkBoxOnlySuccess = new System.Windows.Forms.CheckBox();
            this.buttonUnPauseSession = new System.Windows.Forms.Button();
            this.textBoxSessionName = new System.Windows.Forms.TextBox();
            this.labelSessionName = new System.Windows.Forms.Label();
            this.groupBoxSessionSettings = new System.Windows.Forms.GroupBox();
            this.labelSessionContent = new System.Windows.Forms.Label();
            this.textBoxSessionContent = new System.Windows.Forms.TextBox();
            this.groupBoxSessionSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSessionStarter
            // 
            this.buttonSessionStarter.Location = new System.Drawing.Point(12, 12);
            this.buttonSessionStarter.Name = "buttonSessionStarter";
            this.buttonSessionStarter.Size = new System.Drawing.Size(163, 23);
            this.buttonSessionStarter.TabIndex = 0;
            this.buttonSessionStarter.Text = "Start a log session";
            this.buttonSessionStarter.UseVisualStyleBackColor = true;
            this.buttonSessionStarter.Click += new System.EventHandler(this.ButtonSessionStarter_Click);
            // 
            // checkBoxSupressWebhooks
            // 
            this.checkBoxSupressWebhooks.AutoSize = true;
            this.checkBoxSupressWebhooks.Checked = true;
            this.checkBoxSupressWebhooks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSupressWebhooks.Location = new System.Drawing.Point(9, 58);
            this.checkBoxSupressWebhooks.Name = "checkBoxSupressWebhooks";
            this.checkBoxSupressWebhooks.Size = new System.Drawing.Size(271, 17);
            this.checkBoxSupressWebhooks.TabIndex = 1;
            this.checkBoxSupressWebhooks.Text = "suppress Discord webhooks until session concludes";
            this.checkBoxSupressWebhooks.UseVisualStyleBackColor = true;
            // 
            // checkBoxOnlySuccess
            // 
            this.checkBoxOnlySuccess.AutoSize = true;
            this.checkBoxOnlySuccess.Checked = true;
            this.checkBoxOnlySuccess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOnlySuccess.Location = new System.Drawing.Point(9, 81);
            this.checkBoxOnlySuccess.Name = "checkBoxOnlySuccess";
            this.checkBoxOnlySuccess.Size = new System.Drawing.Size(141, 17);
            this.checkBoxOnlySuccess.TabIndex = 2;
            this.checkBoxOnlySuccess.Text = "only successful attempts";
            this.checkBoxOnlySuccess.UseVisualStyleBackColor = true;
            // 
            // buttonUnPauseSession
            // 
            this.buttonUnPauseSession.Enabled = false;
            this.buttonUnPauseSession.Location = new System.Drawing.Point(181, 12);
            this.buttonUnPauseSession.Name = "buttonUnPauseSession";
            this.buttonUnPauseSession.Size = new System.Drawing.Size(116, 23);
            this.buttonUnPauseSession.TabIndex = 3;
            this.buttonUnPauseSession.Text = "Pause session";
            this.buttonUnPauseSession.UseVisualStyleBackColor = true;
            this.buttonUnPauseSession.Click += new System.EventHandler(this.ButtonUnPauseSession_Click);
            // 
            // textBoxSessionName
            // 
            this.textBoxSessionName.Location = new System.Drawing.Point(9, 32);
            this.textBoxSessionName.Name = "textBoxSessionName";
            this.textBoxSessionName.Size = new System.Drawing.Size(270, 20);
            this.textBoxSessionName.TabIndex = 4;
            // 
            // labelSessionName
            // 
            this.labelSessionName.AutoSize = true;
            this.labelSessionName.Location = new System.Drawing.Point(6, 16);
            this.labelSessionName.Name = "labelSessionName";
            this.labelSessionName.Size = new System.Drawing.Size(106, 13);
            this.labelSessionName.TabIndex = 5;
            this.labelSessionName.Text = "Name of the session:";
            // 
            // groupBoxSessionSettings
            // 
            this.groupBoxSessionSettings.Controls.Add(this.textBoxSessionContent);
            this.groupBoxSessionSettings.Controls.Add(this.labelSessionContent);
            this.groupBoxSessionSettings.Controls.Add(this.textBoxSessionName);
            this.groupBoxSessionSettings.Controls.Add(this.labelSessionName);
            this.groupBoxSessionSettings.Controls.Add(this.checkBoxOnlySuccess);
            this.groupBoxSessionSettings.Controls.Add(this.checkBoxSupressWebhooks);
            this.groupBoxSessionSettings.Location = new System.Drawing.Point(12, 41);
            this.groupBoxSessionSettings.Name = "groupBoxSessionSettings";
            this.groupBoxSessionSettings.Size = new System.Drawing.Size(285, 146);
            this.groupBoxSessionSettings.TabIndex = 6;
            this.groupBoxSessionSettings.TabStop = false;
            this.groupBoxSessionSettings.Text = "Session settings";
            // 
            // labelSessionContent
            // 
            this.labelSessionContent.AutoSize = true;
            this.labelSessionContent.Location = new System.Drawing.Point(6, 101);
            this.labelSessionContent.Name = "labelSessionContent";
            this.labelSessionContent.Size = new System.Drawing.Size(91, 13);
            this.labelSessionContent.TabIndex = 6;
            this.labelSessionContent.Text = "Discord message:";
            // 
            // textBoxSessionContent
            // 
            this.textBoxSessionContent.Location = new System.Drawing.Point(9, 117);
            this.textBoxSessionContent.Name = "textBoxSessionContent";
            this.textBoxSessionContent.Size = new System.Drawing.Size(270, 20);
            this.textBoxSessionContent.TabIndex = 7;
            // 
            // FormLogSession
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 196);
            this.Controls.Add(this.groupBoxSessionSettings);
            this.Controls.Add(this.buttonUnPauseSession);
            this.Controls.Add(this.buttonSessionStarter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLogSession";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Log sessions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLogSession_FormClosing);
            this.groupBoxSessionSettings.ResumeLayout(false);
            this.groupBoxSessionSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonSessionStarter;
        public System.Windows.Forms.CheckBox checkBoxSupressWebhooks;
        public System.Windows.Forms.CheckBox checkBoxOnlySuccess;
        private System.Windows.Forms.Button buttonUnPauseSession;
        private System.Windows.Forms.Label labelSessionName;
        private System.Windows.Forms.GroupBox groupBoxSessionSettings;
        public System.Windows.Forms.TextBox textBoxSessionName;
        private System.Windows.Forms.Label labelSessionContent;
        public System.Windows.Forms.TextBox textBoxSessionContent;
    }
}