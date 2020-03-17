﻿using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using PlenBotLogUploader.Tools;

namespace PlenBotLogUploader
{
    public partial class FormLogSession : Form
    {
        #region definitions
        // properties
        public bool SessionRunning { get; private set; } = false;

        // fields
        private FormMain mainLink;
        private bool sessionPaused = false;
        private readonly Stopwatch stopWatch = new Stopwatch();
        private DateTime sessionTimeStarted;
        #endregion

        public FormLogSession(FormMain mainLink)
        {
            this.mainLink = mainLink;
            InitializeComponent();
            Icon = Properties.Resources.AppIcon;
        }

        private void FormLogSession_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            Properties.Settings.Default.SessionName = textBoxSessionName.Text;
            Properties.Settings.Default.SessionMessage = textBoxSessionContent.Text;
        }

        private async void ButtonSessionStarter_Click(object sender, EventArgs e)
        {
            if (SessionRunning || sessionPaused)
            {
                buttonSessionStarter.Text = "Start a log session";
                buttonUnPauseSession.Text = "Pause session";
                buttonUnPauseSession.Enabled = false;
                SessionRunning = false;
                sessionPaused = false;
                stopWatch.Stop();
                string elapsedTime = NiceTime.ParseTimeSpanHMS(stopWatch.Elapsed);
                int sortBy = radioButtonSortByUpload.Checked ? 1 : 0;
                var logSessionSettings = new LogSessionSettings()
                {
                    Name = textBoxSessionName.Text,
                    ContentText = textBoxSessionContent.Text,
                    ShowSuccess = !checkBoxOnlySuccess.Checked,
                    ElapsedTime = elapsedTime,
                    SortBy = (LogSessionSortBy)sortBy
                };
                var fileName = $"{textBoxSessionName.Text.ToLower().Replace(" ", "")} {sessionTimeStarted.Year}-{sessionTimeStarted.Month}-{sessionTimeStarted.Day} {sessionTimeStarted.Hour}-{sessionTimeStarted.Minute}-{sessionTimeStarted.Second}";
                File.AppendAllText($"{mainLink.LocalDir}{fileName}.csv", "Boss;BossId;Success;Duration;RecordedBy;EliteInsightsVersion;arcdpsVersion;Permalink\n");
                foreach (var reportJSON in mainLink.SessionLogs)
                {
                    string success = (reportJSON.Encounter.Success ?? false) ? "true" : "false";
                    File.AppendAllText($"{mainLink.LocalDir}{fileName}.csv",
                        $"{reportJSON.ExtraJSON?.FightName ?? reportJSON.Encounter.Boss};{reportJSON.Encounter.BossId};{success};{reportJSON.ExtraJSON?.Duration ?? ""};{reportJSON.ExtraJSON?.RecordedBy ?? ""};{reportJSON.ExtraJSON?.EliteInsightsVersion ?? ""};{reportJSON.EVTC.Type}{reportJSON.EVTC.Version};{reportJSON.Permalink}\n");
                }
                await mainLink.ExecuteSessionLogWebhooksAsync(logSessionSettings);
                mainLink.SessionLogs.Clear();
            }
            else
            {
                buttonSessionStarter.Text = "Stop the log session";
                buttonUnPauseSession.Text = "Pause session";
                buttonUnPauseSession.Enabled = true;
                SessionRunning = true;
                sessionPaused = false;
                stopWatch.Start();
                sessionTimeStarted = DateTime.Now;
            }
        }

        private void ButtonUnPauseSession_Click(object sender, EventArgs e)
        {
            if (!sessionPaused)
            {
                SessionRunning = false;
                sessionPaused = true;
                buttonUnPauseSession.Text = "Unpause session";
            }
            else
            {
                SessionRunning = true;
                sessionPaused = false;
                buttonUnPauseSession.Text = "Pause session";
            }
        }

        public void CheckBoxSupressWebhooks_CheckedChanged(object sender, EventArgs e) => Properties.Settings.Default.SessionSuppressWebhooks = checkBoxSupressWebhooks.Checked;

        public void CheckBoxOnlySuccess_CheckedChanged(object sender, EventArgs e) => Properties.Settings.Default.SessionOnlySuccess = checkBoxOnlySuccess.Checked;

        public void CheckBoxSaveToFile_CheckedChanged(object sender, EventArgs e) => Properties.Settings.Default.SessionSaveToFile = checkBoxSaveToFile.Checked;

        private void RadioButtonSortByWing_CheckedChanged(object sender, EventArgs e) => Properties.Settings.Default.SessionSort = 0;

        private void RadioButtonSortByUpload_CheckedChanged(object sender, EventArgs e) => Properties.Settings.Default.SessionSort = 1;
    }
}
