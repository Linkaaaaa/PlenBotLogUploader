﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Microsoft.Win32;
using TwitchIRCClient;
using PlenBotLogUploader.DPSReport;

namespace PlenBotLogUploader
{
    public partial class FormMain : Form
    {
        // properties
        public RegistryKey RegistryAccess { get; set; } = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Plenyx\PlenBotUploader");
        public List<string> Logs { get; set; } = new List<string>();
        public string LogsLocation { get; set; } = "";
        public DPSReportJSONMinimal LastLog { get; set; }
        public string ChannelName { get; set; } = "";
        public string DPSReportServer { get; set; } = "";
        public Dictionary<int, BossData> allBosses = Bosses.GetBossesAsDictionary();
        public int Build { get; } = 11;

        // fields
        private TwitchIrcClient chatConnect;
        private const int minFileSize = 20480;
        private FileSystemWatcher watcher = new FileSystemWatcher() { Filter = "*.*", IncludeSubdirectories = true, NotifyFilter = NotifyFilters.FileName };
        private FormPing pingLink;
        private FormTwitchNameSetup twitchNameLink;
        private FormDPSReportServer dpsReportServerLink;
        private HttpClient webClient = new HttpClient();

        public FormMain()
        {
            InitializeComponent();
            Text = $"{Text} b{Build}";
            pingLink = new FormPing(this);
            twitchNameLink = new FormTwitchNameSetup(this);
            dpsReportServerLink = new FormDPSReportServer(this);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            new Thread(NewBuildCheck).Start();
            try
            {
                if (RegistryAccess.GetValue("logsLocation") == null)
                {
                    RegistryAccess.SetValue("logsLocation", "");
                }
                if (RegistryAccess.GetValue("channel") == null)
                {
                    RegistryAccess.SetValue("channel", "");
                }
                if (RegistryAccess.GetValue("uploadAll") == null)
                {
                    RegistryAccess.SetValue("uploadAll", 1);
                }
                if (RegistryAccess.GetValue("uploadToTwitch") == null)
                {
                    RegistryAccess.SetValue("uploadToTwitch", 1);
                }
                if (RegistryAccess.GetValue("uploadIgnoreSize") == null)
                {
                    RegistryAccess.SetValue("uploadIgnoreSize", 0);
                }
                if (RegistryAccess.GetValue("wepSkill1") == null)
                {
                    RegistryAccess.SetValue("wepSkill1", 1);
                }
                if (RegistryAccess.GetValue("dpsReportServer") == null)
                {
                    RegistryAccess.SetValue("dpsReportServer", 0);
                }
                if (RegistryAccess.GetValue("trayEnabled") == null)
                {
                    RegistryAccess.SetValue("trayEnabled", 1);
                }
                if (RegistryAccess.GetValue("trayMinimise") == null)
                {
                    RegistryAccess.SetValue("trayMinimise", 0);
                }
                if (RegistryAccess.GetValue("trayInfo") == null)
                {
                    RegistryAccess.SetValue("trayInfo", 1);
                }
                if (RegistryAccess.GetValue("remotePingEnabled") == null)
                {
                    RegistryAccess.SetValue("remotePingEnabled", 0);
                    RegistryAccess.SetValue("remotePingMethod", 0);
                    RegistryAccess.SetValue("remotePingURL", "");
                    RegistryAccess.SetValue("remotePingSign", "");
                }
                LogsLocation = (string)RegistryAccess.GetValue("logsLocation", "");
                if (LogsLocation == "")
                {
                    labelLocationInfo.Text = "!!! Select a directory with the logs !!!";
                }
                else
                {
                    if (Directory.Exists(LogsLocation))
                    {
                        LogsScan(LogsLocation);
                        watcher.Path = LogsLocation;
                        watcher.Renamed += OnLogCreated;
                        watcher.EnableRaisingEvents = true;
                        buttonOpenLogs.Enabled = true;
                    }
                    else
                    {
                        RegistryAccess.SetValue("logsLocation", "");
                        labelLocationInfo.Text = "!!! Select a directory with the logs !!!";
                    }
                }
                ChannelName = ((string)RegistryAccess.GetValue("channel", "")).ToLower();
                if (!string.IsNullOrEmpty(ChannelName))
                {
                    twitchNameLink.textBoxChannelUrl.Text = $"https://twitch.tv/{ChannelName}/";
                }
                if ((int)RegistryAccess.GetValue("dpsReportServer", 0) == 0)
                {
                    DPSReportServer = "dps.report";
                }
                else
                {
                    DPSReportServer = "a.dps.report";
                    dpsReportServerLink.radioButtonA.Checked = true;
                }
                if ((int)RegistryAccess.GetValue("uploadAll", 0) == 1)
                {
                    checkBoxUploadLogs.Checked = true;
                    checkBoxPostToTwitch.Enabled = true;
                    toolStripMenuItemUploadLogs.Checked = true;
                    toolStripMenuItemPostToTwitch.Enabled = true;
                }
                if ((int)RegistryAccess.GetValue("uploadToTwitch", 0) == 1)
                {
                    checkBoxPostToTwitch.Checked = true;
                    checkBoxPostToTwitch.Enabled = true;
                    toolStripMenuItemPostToTwitch.Checked = true;
                    toolStripMenuItemPostToTwitch.Enabled = true;
                }
                if ((int)RegistryAccess.GetValue("uploadIgnoreSize", 0) == 1)
                {
                    checkBoxFileSizeIgnore.Checked = true;
                }
                if ((int)RegistryAccess.GetValue("wepSkill1", 0) == 1)
                {
                    checkBoxWepSkill1.Checked = true;
                }
                if ((int)RegistryAccess.GetValue("trayEnabled", 0) == 1)
                {
                    checkBoxTrayEnable.Checked = true;
                    checkBoxTrayMinimiseToIcon.Enabled = true;
                    checkBoxTrayNotification.Enabled = true;
                    notifyIconTray.Visible = true;
                }
                if ((int)RegistryAccess.GetValue("trayMinimise", 0) == 1 && checkBoxTrayEnable.Checked)
                {
                    checkBoxTrayMinimiseToIcon.Checked = true;
                }
                if ((int)RegistryAccess.GetValue("trayInfo", 0) == 1 && checkBoxTrayEnable.Checked)
                {
                    checkBoxTrayNotification.Checked = true;
                }
                if ((int)RegistryAccess.GetValue("remotePingEnabled", 0) == 1)
                {
                    pingLink.checkBoxEnablePing.Checked = true;
                    int method = (int)RegistryAccess.GetValue("remotePingMethod", 0);
                    if (method == 0)
                    {
                        pingLink.radioButtonMethodGet.Checked = true;
                    }
                    else if (method == 1)
                    {
                        pingLink.radioButtonMethodPost.Checked = true;
                    }
                    pingLink.textBoxURL.Text = (string)RegistryAccess.GetValue("remotePingURL", "");
                    pingLink.textBoxSign.Text = (string)RegistryAccess.GetValue("remotePingSign", "");
                }
                if (RegistryAccess.GetValue("firstSetup") == null)
                {
                    MessageBox.Show("It looks like this is the first time you are running this program.\nIf you have any issues feel free to contact me directly by Twitch, Discord (@Plenyx#1029) or on GitHub!\n\nPlenyx", "Thank you for using PlenBotLogUploader", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    twitchNameLink.Show();
                    RegistryAccess.SetValue("firstSetup", 1);
                }
                chatConnect = new TwitchIrcClient("gw2loguploader", "oauth:ycgqr3dyef7gp5r8uk7d5jz30nbrc6");
                chatConnect.ReceiveMessage += ReadMessages;
                chatConnect.StateChange += OnIrcStateChanged;
                chatConnect.BeginConnection();
                new Thread(DoCommandArgs).Start();
                /* Subscribe to field changes events, otherwise they would trigger with load */
                checkBoxPostToTwitch.CheckedChanged += new EventHandler(checkBoxPostToTwitch_CheckedChanged);
                checkBoxWepSkill1.CheckedChanged += new EventHandler(checkBoxWepSkill1_CheckedChanged);
                checkBoxUploadLogs.CheckedChanged += new EventHandler(checkBoxUploadAll_CheckedChanged);
                checkBoxFileSizeIgnore.CheckedChanged += new EventHandler(checkBoxFileSizeIgnore_CheckedChanged);
                checkBoxTrayNotification.CheckedChanged += new EventHandler(checkBoxTrayNotification_CheckedChanged);
                checkBoxTrayMinimiseToIcon.CheckedChanged += new EventHandler(checkBoxTrayMinimiseToIcon_CheckedChanged);
                checkBoxTrayEnable.CheckedChanged += new EventHandler(checkBoxTrayEnable_CheckedChanged);
            }
            catch
            {
                Registry.CurrentUser.DeleteSubKey(@"SOFTWARE\Plenyx\PlenBotUploader");
                MessageBox.Show("An error in the Windows' registry has occurred.\nAll settings are reset.\nTry running the application again.", "An error has occurred");
                Close();
            }
        }

        // triggeres when a file is renamed within the folder, renaming is the last process done by arcdps to create evtc or zevtc files
        private void OnLogCreated(object sender, FileSystemEventArgs e)
        {
            if (!Logs.Contains(e.FullPath) && (e.FullPath.EndsWith(".evtc") || e.FullPath.EndsWith(".zevtc")))
            {
                Logs.Add(e.FullPath);
                if (checkBoxUploadLogs.Checked)
                {
                    try
                    {
                        if (checkBoxFileSizeIgnore.Checked || new FileInfo(e.FullPath).Length >= minFileSize)
                        {
                            string zipfilelocation = e.FullPath;
                            bool archived = false;
                            if (!e.FullPath.EndsWith(".zevtc"))
                            {
                                zipfilelocation = $"{GetLocalDir()}{Path.GetFileName(e.FullPath)}.zevtc";
                                using (ZipArchive zipfile = ZipFile.Open(zipfilelocation, ZipArchiveMode.Create)) { zipfile.CreateEntryFromFile(@e.FullPath, Path.GetFileName(e.FullPath)); }
                                archived = true;
                            }
                            try
                            {
                                Dictionary<string, string> postData = new Dictionary<string, string>
                                {
                                    { "generator", "ei" },
                                    { "json", "1" }
                                };
                                if (checkBoxWepSkill1.Checked)
                                {
                                    postData.Add("rotation_weap1", "1");
                                }
                                HttpUploadFileToDPSReport(zipfilelocation, postData);
                            }
                            catch
                            {
                                throw;
                            }
                            finally
                            {
                                if (archived)
                                {
                                    File.Delete($"{GetLocalDir()}{Path.GetFileName(e.FullPath)}.zevtc");
                                }
                            }
                        }
                    }
                    catch
                    {
                        Logs.Remove(e.FullPath);
                        AddToText("Unable to upload the file: " + e.FullPath);
                    }
                }
                UpdateLogCount();
            }
        }

        private void ShowBalloon(string title, string description, int ms) => notifyIconTray.ShowBalloonTip(ms, title, description, ToolTipIcon.Info);

        protected async void NewBuildCheck()
        {
            try
            {
                string response = await DownloadFileAsyncToString("https://raw.githubusercontent.com/Plenyx/PlenBotLogUploader/master/VERSION");
                if (int.TryParse(response, out int currentversion))
                {
                    if (currentversion > Build)
                    {
                        AddToText($">>");
                        AddToText($">>> New build available (build n.{response})");
                        AddToText("https://github.com/Plenyx/PlenBotLogUploader/releases/");
                        AddToText($">>");
                        DialogResult result = MessageBox.Show("Do you want to download the newest version?", $"New build available (build n.{response})", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes)
                        {
                            Process.Start("https://github.com/Plenyx/PlenBotLogUploader/releases/");
                        }
                    }
                }
            }
            catch { /* do nothing */ }
        }

        protected void DoCommandArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                Dictionary<string, string> postData = new Dictionary<string, string>
                {
                    { "generator", "ei" },
                    { "json", "1" }
                };
                if (checkBoxWepSkill1.Checked)
                {
                    postData.Add("rotation_weap1", "1");
                }
                foreach (string arg in args)
                {
                    if (arg == Application.ExecutablePath)
                    {
                        continue;
                    }
                    if (File.Exists(arg) && (arg.EndsWith(".evtc") || arg.EndsWith(".zevtc")))
                    {
                        bool archived = false;
                        string zipfilelocation = arg;
                        if (!arg.EndsWith(".zevtc"))
                        {
                            zipfilelocation = $"{GetLocalDir()}{Path.GetFileName(arg)}.zevtc";
                            using (ZipArchive zipfile = ZipFile.Open(zipfilelocation, ZipArchiveMode.Create)) { zipfile.CreateEntryFromFile(@arg, Path.GetFileName(arg)); }
                            archived = true;
                        }
                        try
                        {
                            HttpUploadFileToDPSReport(zipfilelocation, postData);
                        }
                        catch
                        {
                            AddToText($">>> Unknown error uploading a log: {zipfilelocation}");
                        }
                        finally
                        {
                            if (archived)
                            {
                                File.Delete($"{GetLocalDir()}{Path.GetFileName(zipfilelocation)}.zevtc");
                            }
                        }
                    }
                }
            }
        }

        public string GetLocalDir() => $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Remove(0, 8))}\\";

        private void AddToText(string s)
        {
            if (textBoxUploadInfo.InvokeRequired)
            {
                // invokes the same function on the main thread
                textBoxUploadInfo.Invoke((Action<string>)delegate (string text) { AddToText(text); }, s);
            }
            else
            {
                textBoxUploadInfo.AppendText(s + Environment.NewLine);
                textBoxUploadInfo.SelectionStart = textBoxUploadInfo.TextLength;
                textBoxUploadInfo.ScrollToCaret();
            }
            
        }

        private void UpdateLogCount()
        {
            if (labelLocationInfo.InvokeRequired)
            {
                // invokes the same function on the main thread
                labelLocationInfo.Invoke((Action)delegate () { UpdateLogCount(); });
            }
            else
            {
                labelLocationInfo.Text = $"Logs in the directory: {Logs.Count}";
            }
        }

        public async Task<string> DownloadFileAsyncToString(string url)
        {
            var responseCode = await webClient.GetAsync(new Uri(url));
            try
            {
                return await responseCode.Content.ReadAsStringAsync();
            }
            catch
            {
                return "";
            }
        }

        public async void HttpUploadFileToDPSReport(string file, Dictionary<string, string> postData, bool bypassMessage = false)
        {
            var content = new MultipartFormDataContent();
            foreach (string key in postData.Keys)
            {
                content.Add(new StringContent(postData[key]), key);
            }
            using (FileStream inputStream = File.OpenRead(file))
            {
                using (StreamContent contentStream = new StreamContent(inputStream))
                {
                    content.Add(contentStream, "file", Path.GetFileName(file));
                    try
                    {
                        using (var responseMessage = await webClient.PostAsync(new Uri($"https://{DPSReportServer}/uploadContent"), content))
                        {
                            string response = await responseMessage.Content.ReadAsStringAsync();
                            try
                            {
                                DPSReportJSONMinimal reportJSON = new JavaScriptSerializer().Deserialize<DPSReportJSONMinimal>(response);
                                File.AppendAllText($"{GetLocalDir()}logs.txt", $"{reportJSON.permalink}\n");
                                if (checkBoxPostToTwitch.Checked && !bypassMessage)
                                {
                                    AddToText($"New log: {reportJSON.permalink}");
                                    LastLog = reportJSON;
                                    if (allBosses.ContainsKey(reportJSON.encounter.bossId))
                                    {
                                        if (!Bosses.IsGolem(reportJSON.encounter.bossId))
                                        {
                                            string format = $"Link to the {allBosses[reportJSON.encounter.bossId].Name}";
                                            if (reportJSON.encounter.success ?? false)
                                            {
                                                format = $"{format} {allBosses[reportJSON.encounter.bossId].SuccessMsg}";
                                            }
                                            else
                                            {
                                                format = $"{format} {allBosses[reportJSON.encounter.bossId].FailMsg}";
                                            }
                                            await chatConnect.SendChatMessage(ChannelName, $"{format}: {reportJSON.permalink}");
                                        }
                                        else
                                        {
                                            await chatConnect.SendChatMessage(ChannelName, $"Golem log: {reportJSON.permalink}");
                                        }
                                    }
                                    else
                                    {
                                        await chatConnect.SendChatMessage(ChannelName, $"Link to the log: {reportJSON.permalink}");
                                    }
                                }
                                else
                                {
                                    AddToText(reportJSON.permalink);
                                }
                                await PingServer(reportJSON);
                            }
                            catch
                            {
                                AddToText($"Unable to upload file {file}, dps.report responded with invalid permanent link");
                            }
                        }
                    }
                    catch
                    {
                        AddToText($"Unable to upload file {file}, dps.report not responding");
                    }
                }
            }
        }

        public async Task PingServer(DPSReportJSONMinimal reportJSON)
        {
            if (pingLink.checkBoxEnablePing.Checked)
            {
                if (pingLink.radioButtonMethodPost.Checked)
                {
                    Dictionary<string, string> fields = new Dictionary<string, string>
                    {
                        { "permalink", reportJSON.permalink },
                        { "bossId", reportJSON.encounter.bossId.ToString() },
                        { "success", (reportJSON.encounter.success ?? false) ? "1" : "0" },
                        { "sign", pingLink.textBoxSign.Text }
                    };
                    FormUrlEncodedContent content = new FormUrlEncodedContent(fields);
                    try
                    {
                        var response = await webClient.PostAsync(pingLink.textBoxURL.Text.ToString(), content);
                        AddToText("Log pinged.");
                    }
                    catch
                    {
                        AddToText("Unable to ping the server, check the settings or the server is not responding.");
                    }
                }
                else if (pingLink.radioButtonMethodGet.Checked)
                {
                    try
                    {
                        string success = (reportJSON.encounter.success ?? false) ? "1" : "0";
                        string encounterInfo = $"bossId={reportJSON.encounter.bossId.ToString()}&success={success}&permalink={System.Web.HttpUtility.UrlEncode(reportJSON.permalink)}";
                        if (pingLink.textBoxURL.Text.Contains("?"))
                        {
                            var response = await webClient.GetAsync($"{pingLink.textBoxURL.Text}&sign={pingLink.textBoxSign.Text}&{encounterInfo}");
                        }
                        else
                        {
                            var response = await webClient.GetAsync($"{pingLink.textBoxURL.Text}?sign={pingLink.textBoxSign.Text}&{encounterInfo}");
                        }
                        AddToText("Log pinged.");
                    }
                    catch
                    {
                        AddToText("Unable to ping the server, check the settings or the server is not responding.");
                    }
                }
            }
        }

        private void LogsScan(string directory)
        {
            foreach (string f in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                if (f.EndsWith(".evtc") || f.EndsWith(".zevtc"))
                {
                    Logs.Add(f);
                }
            }
            UpdateLogCount();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            RegistryAccess.Flush();
            RegistryAccess.Dispose();
            chatConnect.Dispose();
            watcher.Dispose();
            webClient.Dispose();
        }

        private void checkBoxUploadAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUploadLogs.Checked)
            {
                RegistryAccess.SetValue("uploadAll", 1);
                toolStripMenuItemUploadLogs.Checked = true;
                checkBoxPostToTwitch.Enabled = true;
                toolStripMenuItemPostToTwitch.Enabled = true;
            }
            else
            {
                RegistryAccess.SetValue("uploadAll", 0);
                toolStripMenuItemUploadLogs.Checked = false;
                checkBoxPostToTwitch.Enabled = false;
                checkBoxPostToTwitch.Checked = false;
                toolStripMenuItemPostToTwitch.Enabled = false;
                toolStripMenuItemPostToTwitch.Checked = false;
            }
        }

        public void ReconnectBot()
        {
            chatConnect.ReceiveMessage -= ReadMessages;
            chatConnect.StateChange -= OnIrcStateChanged;
            chatConnect.Dispose();
            chatConnect = null;
            chatConnect = new TwitchIrcClient("gw2loguploader", "oauth:ycgqr3dyef7gp5r8uk7d5jz30nbrc6");
            chatConnect.ReceiveMessage += ReadMessages;
            chatConnect.StateChange += OnIrcStateChanged;
            chatConnect.BeginConnection();
        }

        private void buttonReconnectBot_Click(object sender, EventArgs e) => ReconnectBot();

        private void buttonLogsLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Select the arcdps folder containing the combat logs.\nThe folder's name you are looking for is \"arcdps.cbtlogs\""
            };
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                if (dialog.SelectedPath.Contains("arcdps.cbtlogs"))
                {
                    LogsLocation = dialog.SelectedPath;
                    RegistryAccess.SetValue("logsLocation", LogsLocation);
                    Logs.Clear();
                    LogsScan(LogsLocation);
                    watcher.Renamed -= OnLogCreated;
                    watcher.Dispose();
                    watcher = null;
                    watcher = new FileSystemWatcher()
                    {
                        Path = LogsLocation,
                        Filter = "*.*",
                        IncludeSubdirectories = true,
                        NotifyFilter = NotifyFilters.FileName
                    };
                    watcher.Renamed += OnLogCreated;
                    watcher.EnableRaisingEvents = true;
                    buttonOpenLogs.Enabled = true;
                }
                else
                {
                    MessageBox.Show("The specified location does not appear to be an arcdps folder.\nCheck your directory and try again.", "An error has occurred");
                }
            }
            dialog.Dispose();
        }

        private void checkBoxTrayEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTrayEnable.Checked)
            {
                checkBoxTrayMinimiseToIcon.Enabled = true;
                checkBoxTrayNotification.Enabled = true;
                RegistryAccess.SetValue("trayEnabled", 1);
                notifyIconTray.Visible = true;
            }
            else
            {
                checkBoxTrayMinimiseToIcon.Enabled = false;
                checkBoxTrayNotification.Enabled = false;
                checkBoxTrayMinimiseToIcon.Checked = false;
                checkBoxTrayNotification.Checked = false;
                RegistryAccess.SetValue("trayEnabled", 0);
                RegistryAccess.SetValue("trayMinimise", 0);
                RegistryAccess.SetValue("trayInfo", 0);
                notifyIconTray.Visible = false;
            }
        }

        private void checkBoxWepSkill1_CheckedChanged(object sender, EventArgs e) => RegistryAccess.SetValue("wepSkill1", checkBoxUploadLogs.Checked ? 1 : 0);

        private void checkBoxTrayMinimiseToIcon_CheckedChanged(object sender, EventArgs e) => RegistryAccess.SetValue("trayMinimise", checkBoxTrayMinimiseToIcon.Checked ? 1 : 0);

        private void checkBoxTrayNotification_CheckedChanged(object sender, EventArgs e)
        {
            RegistryAccess.SetValue("trayInfo", checkBoxTrayNotification.Checked ? 1 : 0);
            if (checkBoxTrayNotification.Checked)
            {
                ShowBalloon("Tray information", "Tray information enabled.", 4500);
            }
        }

        private void checkBoxPostToTwitch_CheckedChanged(object sender, EventArgs e)
        {
            RegistryAccess.SetValue("uploadToTwitch", checkBoxPostToTwitch.Checked ? 1 : 0);
            toolStripMenuItemPostToTwitch.Checked = checkBoxPostToTwitch.Checked;
        }

        private void checkBoxFileSizeIgnore_CheckedChanged(object sender, EventArgs e) => RegistryAccess.SetValue("uploadIgnoreSize", checkBoxFileSizeIgnore.Checked ? 1 : 0);

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if ((WindowState == FormWindowState.Minimized) && checkBoxTrayMinimiseToIcon.Checked)
            {
                ShowInTaskbar = false;
            }
        }

        private void notifyIconTray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ShowInTaskbar)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
                WindowState = FormWindowState.Normal;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) => notifyIconTray.Visible = false;

        protected async void OnIrcStateChanged(object sender, IrcChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case IrcStates.Disconnected:
                    AddToText("> DISCONNECTED FROM TWITCH");
                    break;
                case IrcStates.Connecting:
                    AddToText("> BOT CONNECTING TO TWITCH");
                    break;
                case IrcStates.Connected:
                    AddToText("> CONNECTION ESTABILISHED");
                    if (ChannelName != "")
                    {
                        await chatConnect.JoinRoom(ChannelName);
                    }
                    break;
                case IrcStates.ChannelJoining:
                    AddToText($"> JOINING CHANNEL {e.Channel.ToUpper()}");
                    break;
                case IrcStates.ChannelJoined:
                    AddToText("> CHANNEL JOINED");
                    break;
                case IrcStates.ChannelLeaving:
                    AddToText($"> LEAVING CHANNEL {e.Channel.ToUpper()}");
                    break;
                default:
                    AddToText("> Unrecognised state received");
                    break;
            }
        }

        protected async void ReadMessages(object sender, IrcMessageEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            string[] messageSplit = e.Message.Split(new string[] { $"#{ChannelName} :" }, StringSplitOptions.None);
            if (messageSplit.Length > 1)
            {
                string command = messageSplit[1].Split(' ')[0].ToLower();
                if (command.Equals("!uploader"))
                {
                    AddToText("> UPLOADER COMMAND USED");
                    await chatConnect.SendChatMessage(ChannelName, $"PlenBot Log Uploader v1 build n.{Build} | https://github.com/Plenyx/PlenBotLogUploader/releases");
                }
                else if (command.Equals("!lastlog") || command.Equals("!log"))
                {
                    if (LastLog != null)
                    {
                        AddToText("> LAST LOG COMMAND USED");
                        await chatConnect.SendChatMessage(ChannelName, $"Link to the last {LastLog.encounter.boss} log: {LastLog.permalink}");
                    }
                }
            }
        }

        private void buttonPingSettings_Click(object sender, EventArgs e)
        {
            pingLink.Show();
            pingLink.BringToFront();
        }

        private void buttonChangeTwitchChannel_Click(object sender, EventArgs e) => twitchNameLink.Show();

        private void toolStripMenuItemUploadLogs_CheckedChanged(object sender, EventArgs e) => checkBoxUploadLogs.Checked = toolStripMenuItemUploadLogs.Checked;

        private void toolStripMenuItemExit_Click(object sender, EventArgs e) => Close();

        private void toolStripMenuItemPostToTwitch_CheckedChanged(object sender, EventArgs e) => checkBoxPostToTwitch.Checked = toolStripMenuItemPostToTwitch.Checked;

        private void buttonOpenLogs_Click(object sender, EventArgs e) => Process.Start(LogsLocation);

        private void buttonDPSReportServer_Click(object sender, EventArgs e)
        {
            dpsReportServerLink.Show();
            dpsReportServerLink.BringToFront();
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>
            {
                { "generator", "ei" },
                { "json", "1" }
            };
            if (checkBoxWepSkill1.Checked)
            {
                postData.Add("rotation_weap1", "1");
            }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (File.Exists(file) && (file.EndsWith(".evtc") || file.EndsWith(".zevtc")))
                {
                    bool archived = false;
                    string zipfilelocation = file;
                    if (!file.EndsWith(".zevtc"))
                    {
                        zipfilelocation = $"{GetLocalDir()}{Path.GetFileName(file)}.zevtc";
                        using (ZipArchive zipfile = ZipFile.Open(zipfilelocation, ZipArchiveMode.Create)) { zipfile.CreateEntryFromFile(@file, Path.GetFileName(file)); }
                        archived = true;
                    }
                    try
                    {
                        HttpUploadFileToDPSReport(zipfilelocation, postData, false);
                    }
                    catch
                    {
                        AddToText($">>> Unknown error uploading a log: {zipfilelocation}");
                    }
                    finally
                    {
                        if (archived)
                        {
                            File.Delete($"{GetLocalDir()}{Path.GetFileName(zipfilelocation)}.zevtc");
                        }
                    }
                }
            }
        }

        private void buttonChatSettings_Click(object sender, EventArgs e)
        {

        }
    }
}
