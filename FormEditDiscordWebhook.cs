﻿using System.Windows.Forms;
using PlenBotLogUploader.DiscordAPI;

namespace PlenBotLogUploader
{
    public partial class FormEditDiscordWebhook : Form
    {
        #region definitions
        // fields
        private FormDiscordPings discordPingLink;
        private DiscordWebhookData data;
        private int reservedId;
        private bool addNew;
        #endregion

        public FormEditDiscordWebhook(FormDiscordPings discordPingLink, int reservedId, bool addNew, DiscordWebhookData data)
        {
            this.discordPingLink = discordPingLink;
            this.data = data;
            this.reservedId = reservedId;
            this.addNew = addNew;
            InitializeComponent();
            Icon = Properties.Resources.AppIcon;
            if (addNew)
            {
                Text = "Add a new webhook";
            }
            else
            {
                Text = "Edit an existing webhook";
            }
            textBoxName.Text = data?.Name ?? "";
            textBoxUrl.Text = data?.URL ?? "";
            checkBoxOnlySuccess.Checked = data?.OnlySuccess ?? false;
        }

        private void FormEditDiscordWebhook_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (textBoxName.Text != "")
            {
                if (addNew)
                {
                    discordPingLink.AllWebhooks[reservedId] = new DiscordWebhookData() { Active = false, Name = textBoxName.Text, URL = textBoxUrl.Text, OnlySuccess = checkBoxOnlySuccess.Checked };
                    discordPingLink.listViewDiscordWebhooks.Items.Add(new ListViewItem() { Name = reservedId.ToString(), Text = textBoxName.Text, Checked = false });
                }
                else
                {
                    if (discordPingLink.AllWebhooks.ContainsKey(reservedId))
                    {
                        discordPingLink.AllWebhooks[reservedId].Active = data.Active;
                        discordPingLink.AllWebhooks[reservedId].Name = textBoxName.Text;
                        discordPingLink.AllWebhooks[reservedId].URL = textBoxUrl.Text;
                        discordPingLink.AllWebhooks[reservedId].OnlySuccess = checkBoxOnlySuccess.Checked;
                        discordPingLink.listViewDiscordWebhooks.Items[discordPingLink.listViewDiscordWebhooks.Items.IndexOfKey(reservedId.ToString())] = new ListViewItem() { Name = reservedId.ToString(), Text = textBoxName.Text, Checked = data.Active };
                    }
                }
            }
        }
    }
}
