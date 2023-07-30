using System.Diagnostics;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using HOTSLogsUploader.Utilities;
using static Heroes.ReplayParser.DataParser;
using static HOTSLogsUploader.Utilities.ReplayUploader;
using Newtonsoft.Json;
using System.Net.Http;
using HOTSLogsUploader.Common;
using Heroes.ReplayParser;
using Serilog;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Net.Http.Headers;
using System.Web;
using System.Drawing;
using System.Windows.Media;
using System.Drawing.Text;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Permissions;
// ReSharper disable LocalizableElement

namespace HOTSLogsUploader
{
    public partial class Main : Form
    {
        private readonly ReplayDataSource ProcessedReplays = new ReplayDataSource(true);
        private readonly ReplayScanner ReplayScanner = new ReplayScanner();
        public BindingList<ReplayDataItem> DataSource = new BindingList<ReplayDataItem>();
        private static DateTime LastUpdateCheck = DateTime.MinValue;

        private const int NotificationDisplayDurationInMS = 3000;
        private readonly object ThreadLock = new object();

        // NOTIFICATION
        private DateTime displayNextNotification = DateTime.UtcNow;

        //private string replayFolder = Properties.Settings.Default.ReplayDirectory;
        //public static bool automaticReplayUpload = Properties.Settings.Default.AutomaticUpload;

        public Main()
        {
            InitializeComponent();

            // Initialize the logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("ExceptionLog.log")
                .CreateLogger();

            this.CleanupUpdateFiles();

            // reload settings
            Properties.Settings.Default.Reload();

            this.ReplayScanner.ProcessingStage += new ProcessingStage(this.OnProcessStageChange);
            this.updateAvailableToolStripMenuItem.Visible = false;
        }


        private (string DownloadFile, string RunningInstanceCopy) GetUpdateFilenames()
        {
            return (Path.Combine(Application.StartupPath, Constants.UpdateFileName), Path.ChangeExtension(Application.ExecutablePath, Constants.BackupInstanceExt));
        }


        private void CleanupUpdateFiles()
        {
            var (downloadFile, runningInstanceCopy) = GetUpdateFilenames();

            // remove previous instances of auto update files
            if (File.Exists(downloadFile))
            {
                File.Delete(downloadFile);
            }

            if (File.Exists(runningInstanceCopy))
            {
                File.Delete(runningInstanceCopy);
            }
        }


        private async Task<UpdateCheckResponse> UpdateIsAvailable()
        {
            try
            {
                var response = HotsLogsHttpClient.Get($"/api/uploader/updatecheck?version={Application.ProductVersion}");

                return JsonConvert.DeserializeObject<UpdateCheckResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to check for updates or deserialize update response");
                return null;
            }
        }


        private async Task<bool> DownloadUpdate(ShowUpdateMenuItemCallback callback = null)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // download the update
                    var downloadResponse = await client.GetAsync($@"{Constants.hotslogsUrl}/hotslogsuploader/hotslogsuploader.exe");

                    // if the download was successful save to file
                    if (downloadResponse.IsSuccessStatusCode)
                    {
                        try
                        {
                            var (updateFile, runningInstanceCopy) = this.GetUpdateFilenames();

                            // get update file name                    
                            this.CleanupUpdateFiles();

                            // download to file
                            using (var stream = new FileStream(updateFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                await downloadResponse.Content.CopyToAsync(stream);
                            }

                            // rename current application and update
                            File.Move(Application.ExecutablePath, runningInstanceCopy);
                            File.Move(updateFile, Application.ExecutablePath);

                            // Update the UI if needed
                            callback?.Invoke();

                            // Show update notification
                            ShowUpdateNotification();

                            return true;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error downloading update");
                }
            }

            return false;
        }


        public void ResetUploads()
        {
            lock (ThreadLock)
            {
                this.ProcessedReplays.Data.Clear();
                this.ProcessedReplays.Save();
            }

            this.ApplySettings();
        }

        // FORM LOAD
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Change colour and caption when built as debug
#if DEBUG
            var c = System.Drawing.Color.FromArgb(Colors.DarkOrange.A, Colors.DarkOrange.R, Colors.DarkOrange.G, Colors.DarkOrange.B);
            this.Text += " ** DEBUG BUILD **";
            this.BackColor = c;
            this.menuStrip1.BackColor = c;
            this.gvReplayList.BackgroundColor = c;
            this.gvReplayList.DefaultCellStyle.BackColor = c;
#endif

            // Use Check for Updates to verify connection to hotslogs servers
            UpdateCheckResponse updateCheckResponse = await UpdateIsAvailable();
            if (updateCheckResponse == null)
            {
                MessageBox.Show("An error occured while connecting to the HOTS Logs servers. Please check www.hotslogs.com for server issues.", "Error connecting to server");
                Close();
            }

            if (Properties.Settings.Default.AutoUpdate)
            {
                LastUpdateCheck = DateTime.Now;
                if (updateCheckResponse?.NewVersionAvailable ?? false)
                {
                    this.DownloadUpdate(new ShowUpdateMenuItemCallback(ShowUpdateMenuItem)).Forget();
                }
                updateWorker.RunWorkerAsync();
            }


            gvReplayList.AutoGenerateColumns = false;
            gvReplayList.DataSource = this.DataSource;

            if (Properties.Settings.Default.ReplayDirectory == "")
            {
                SetDefaultReplayFolder();
            }
            else
            {
                // SetDefaultReplayFolder also applies settings
                this.ApplySettings();
            }

            ShowBalloonTip("HOTS Logs", "Right-click the icon for more options");
        }


        private void ShowBalloonTip(string title, string text)
        {
            if (Properties.Settings.Default.ShowTrayNotifications && DateTime.UtcNow > displayNextNotification)
            {
                displayNextNotification = DateTime.UtcNow.AddMilliseconds(NotificationDisplayDurationInMS);
                notifyIcon1.BalloonTipTitle = title;
                notifyIcon1.BalloonTipText = text;
                notifyIcon1.ShowBalloonTip(NotificationDisplayDurationInMS);
            }
        }


        private void ShowUpdateNotification()
        {
            notifyIcon1.BalloonTipTitle = "Update Available";
            notifyIcon1.BalloonTipText = "An update has been downloaded. The update will install when you restart the application.";
            notifyIcon1.ShowBalloonTip(NotificationDisplayDurationInMS);
        }


        delegate ReplayDataItem OnNewFileAddedCallback(string filePath);
        private ReplayDataItem OnNewFileAdded(string filePath)
        {
            var result = new ReplayDataItem(filePath);

            if (!this.DataSource.Any(i => i.FileName == result.FileName))
            {
                this.DataSource.Add(result);
            }

            return result;
        }

        private void OnProcessStageChange(object sender, ProcessingStageEventArgs e)
        {
            // Get the current item from the data source, or add a new one if it doesn't exist
            var currentItem =
                this.DataSource.SingleOrDefault(i => i.FilePath == e.FilePath) ??
                this.Invoke(new OnNewFileAddedCallback(OnNewFileAdded), e.FilePath) as ReplayDataItem;

            // update with new details
            currentItem.UploadTimestamp = e.UploadDate;
            currentItem.ParseResult = e.ReplayParseResult;
            if (currentItem.UploadStatus == "")
            {
                currentItem.UploadStatus = e.Stage.AsString();
            }
            // prevent a valid fingerprint from being changed to null            
            currentItem.Fingerprint = e.Fingerprint ?? currentItem.Fingerprint;

            // Once complete, update processed history
            if (e.Stage == ProgressStage.Completed)
            {
                lock (ThreadLock)
                {
                    // If this replay has been processed already, update the processed list
                    var replay = this.ProcessedReplays.Data.SingleOrDefault(i => i.FilePath == e.FilePath);
                    if (replay == null)
                    {
                        // Add to list processed replays
                        replay = new ReplayDataItem(e.FilePath);
                        this.ProcessedReplays.Data.Add(replay);
                    }

                    // Update details
                    replay.Update(currentItem);

                    // Save changes
                    this.ProcessedReplays.Save();
                }

                ShowBalloonTip("HOTS Logs", "Replay Processed: " + Path.GetFileNameWithoutExtension(e.FilePath));
            }
        }


        /// <summary>
        /// Set the default replay folder
        /// </summary>
        private void SetDefaultReplayFolder()
        {
            // Get the "Heroes of the Storm" base folder in Documents
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Heroes of the Storm\\Accounts");

            if (Directory.Exists(baseDir))
            {
                Properties.Settings.Default.ReplayDirectory = baseDir;
            }

            // Save the setting
            Properties.Settings.Default.Save();

            this.ApplySettings();
        }


        private void ChangeGridDataSourceFolder()
        {
            // Clear all data from the grid
            this.DataSource.Clear();

            // Load data if it exists
            if (Directory.Exists(Properties.Settings.Default.ReplayDirectory))
            {
                // Load grid with files from the replay folder
                Directory.GetFiles(Properties.Settings.Default.ReplayDirectory, Constants.ReplayExtensionFilter, SearchOption.AllDirectories)
                    .Where(i => Path.GetDirectoryName(i).EndsWith("Replays\\Multiplayer"))
                    .OrderByDescending(i => i.GetFileDateTime())
                    .ToList()
                    .ForEach(i => DataSource.Add(new ReplayDataItem(i)));

                // For each item in the grid, update it with data from processed replays 
                foreach (ReplayDataItem item in this.DataSource)
                {
                    var savedItem = this.ProcessedReplays.Data.FirstOrDefault(i => i.FilePath == item.FilePath);

                    item.UploadTimestamp = savedItem?.UploadTimestamp ?? null;
                    item.UploadStatus = savedItem?.UploadStatus ?? string.Empty;
                }
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.OnBeforeClose();
        }


        private void StopThreads()
        {
            this.bulkUploadWorker.CancelAsync();
            this.ReplayScanner.EnableRaisingEvents = false;

            while (bulkUploadWorker.IsBusy)
            {
                Application.DoEvents();
            }
        }


        private void OnBeforeClose()
        {
            //this.StopThreads();
            this.ProcessedReplays.Save();
            Properties.Settings.Default.Save();
        }


        private void ApplySettings()
        {
            this.StopThreads();

            if (Directory.Exists(Properties.Settings.Default.ReplayDirectory))
            {
                // rebind data grid
                this.ChangeGridDataSourceFolder();

                // update replay scanner with replay path
                this.ReplayScanner.Path = Properties.Settings.Default.ReplayDirectory;
                this.ReplayScanner.EnableRaisingEvents = true;
                this.ReplayScanner.UploadReplaysAutomatically = Properties.Settings.Default.AutomaticUpload;
                this.ReplayScanner.ShowMatchSummary = Properties.Settings.Default.ShowMatchSummary;

                // start background worker
                this.bulkUploadWorker.RunWorkerAsync();
            }
        }


        delegate void ReportThreadProgressCallback();

        /// <summary>
        /// Background worker to upload replays not previously uploaded
        /// </summary>
        private void bulkUploadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            #region utility methods
            // Call whenever background worker should check to see if it has been cancelled
            void IsThreadCancelled()
            {
                if (e.Cancel)
                {
                    bulkUploadWorker.ReportProgress(0);
                    throw new CancelException();
                }
            }


            var progressSubject = new Subject<(int percent, object userState)>();

            var sub = progressSubject
                // .Sample(TimeSpan.FromSeconds(0.2))
                .Buffer(TimeSpan.FromSeconds(0.2))
                .Subscribe(s =>
                {
                    if (!s.Any())
                    {
                        return;
                    }
                    var percent = s.Last().percent;
                    bulkUploadWorker.ReportProgress(percent, s.Select(x => x.userState).ToList());
                });

            // report work percentage without having to specify bulkUploadWorker
            void ReportThreadProgress(int percent, object userState)
            {
                // Instead of eagerly pushing progress updates, push them into a Subject
                // then sample that subject every 0.2 seconds and send that progress update.

                progressSubject.OnNext((percent, userState));

                //bulkUploadWorker.ReportProgress(percent, userState);
            }
            #endregion

            ConcurrentDictionary<string, Guid> query = new ConcurrentDictionary<string, Guid>();
            IEnumerable<ReplayDataItem> unprocessedReplays;

            try
            {
                // Get unprocessed replays
                lock (ThreadLock)
                {
                    unprocessedReplays = new ConcurrentBag<ReplayDataItem>(this.DataSource
                        .Where(i =>
                            i.UploadStatus != ReplayParseResult.Success.AsString() && i.UploadStatus != ReplayParseResult.Duplicate.AsString() &&
                            (i.UploadStatus == ReplayParseResult.Exception.AsString() || !i.UploadTimestamp.HasValue)));
                }

                // For each replay, get parse result and fingerprint            
                Counter j = new Counter() { WorkCount = unprocessedReplays.Count() };

                // Process fingerprints in parallel, using all avaliable processor threads
                Parallel.ForEach(unprocessedReplays, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, replay =>
                {
                    j++;
                    IsThreadCancelled();

                    ReportThreadProgress(j.Percentage(), "Calculating replay hashes");

                    ReplayParseResult parseResult = replay.ParseResult ?? ReplayParseResult.Exception;
                    Guid? fingerprint = replay.Fingerprint;

                    // If the fingerprint is null or the current parse result is Exception, refresh both
                    if (!fingerprint.HasValue || parseResult == ReplayParseResult.Exception)
                    {
                        (parseResult, fingerprint) = GetReplayFingerprint(replay.FilePath);
                    }

                    // If replay parsed successfully and we have a fingerprint, add to query and update item with fingerprint only
                    if (fingerprint.HasValue && parseResult == ReplayParseResult.Success)
                    {
                        lock (ThreadLock)
                        {
                            replay.Fingerprint = fingerprint;
                        }

                        ReportThreadProgress(j.Percentage(), new ProcessingStageEventArgs(replay.FilePath, ProgressStage.None) { Fingerprint = fingerprint });
                        query.TryAdd(replay.FilePath, replay.Fingerprint.Value);
                    }
                    else
                    {
                        ReportThreadProgress(j.Percentage(), new ProcessingStageEventArgs(replay.FilePath, ProgressStage.Completed, DateTime.UtcNow, parseResult));
                    }
                });

                // Query the status of fingerprint list
                IsThreadCancelled();
                ReportThreadProgress(1, "Checking status of replays");

                HttpResponseMessage responseContent;
                ConcurrentBag<ReplayDataItem> uploadList = new ConcurrentBag<ReplayDataItem>();

                var queryPages = query.Pages(Constants.FingerprintQueryPageSize);

                j.Reset();
                j.WorkCount = queryPages.Count();

                foreach (var page in queryPages)
                {
                    IsThreadCancelled();
                    j++;
                    ReportThreadProgress(j.Percentage(), "Checking status of replays");

                    responseContent = HotsLogsHttpClient.Post("/api/uploader/checkfingerprints", new FingerprintQuery(page.Select(i => i.Value)).ToStringContent());

                    if (responseContent.IsSuccessStatusCode)
                    {
                        var response = FingerprintResponse.Deserialize(responseContent.Content.ReadAsStringAsync().Result);

                        // From the response, update any items that have already been uploaded. Treat any "duplicate" response as a success. Do this before aborting a cancelled thread
                        foreach (var item in response.Fingerprints)
                        {
                            var updateItem = unprocessedReplays.Single(i => i.Fingerprint == item.Hash);
                            if (!item.IsDuplicate)
                            {
                                uploadList.Add(updateItem);
                            }
                            else
                            {
                                ReportThreadProgress(j.Percentage(), new ProcessingStageEventArgs(updateItem.FilePath, ProgressStage.Completed, DateTime.UtcNow, ReplayParseResult.Success));
                            }
                        }
                    }
                }

                // Upload replays in batches
                var uri = HttpUtility.ParseQueryString(string.Empty);
                uri.Add("uploaderVersion", Application.ProductVersion);

                // Upload any replays that are not duplicates of already uploaded replays
                ReportThreadProgress(1, "Uploading replays");
                var uploadPages = uploadList.Pages(Constants.FileUploadPageSize);

                j.Reset();
                j.WorkCount = uploadPages.Count();

                foreach (var page in uploadPages)
                {
                    using (var uploadData = new MultipartFormDataContent())
                    {
                        // add each item to the upload list
                        foreach (var item in page)
                        {
                            j++;
                            IsThreadCancelled();
                            ReportThreadProgress(j.Percentage(), "Uploading replays");

                            var fs = new FileStream(item.FilePath, FileMode.Open, FileAccess.Read);
                            var contentItem = new StreamContent(fs);

                            uploadData.Add(contentItem, Path.GetFileNameWithoutExtension(item.FileName), item.FileName);
                            ReportThreadProgress(j.Percentage(), new ProcessingStageEventArgs(item.FilePath, ProgressStage.Uploading));
                        }

                        // upload group
                        responseContent = HotsLogsHttpClient.Post("/api/uploader/upload", uri, uploadData, true).Result;

                        // Don't terminate thread while processing result
                        if (responseContent.IsSuccessStatusCode)
                        {
                            var responseList = JsonConvert.DeserializeObject<List<UploadResponse>>(responseContent.Content.ReadAsStringAsync().Result);

                            foreach (var item in page)
                            {
                                try
                                {
                                    var uploadedItem = responseList.SingleOrDefault(i => i.Fingerprint == item.Fingerprint);

                                    if (uploadedItem == null)
                                    {
                                        throw new KeyNotFoundException($"No fingerprint match or no fingerprint returned for uploaded file {item.FileName}.");
                                    }

                                    ReportThreadProgress(j.Percentage(), new ProcessingStageEventArgs(item.FilePath, ProgressStage.Completed, DateTime.UtcNow, (ReplayParseResult)uploadedItem.Result));
                                }
                                catch (KeyNotFoundException notFound)
                                {
                                    ReportThreadProgress(j.Percentage(), new ProcessingStageEventArgs(item.FilePath, ProgressStage.Completed, DateTime.UtcNow, ReplayParseResult.Exception));
                                    Log.Error(notFound, "Batch upload");
                                }
                            }

                        }
                    }
                }
            }
            catch (CancelException)
            {
                // save current progress
                return;
            }
            finally
            {
                ReportThreadProgress(0, "Done");
                progressSubject.OnCompleted();
                sub.Dispose();
                progressSubject.Dispose();
            }

            // bulkUploadWorker.ReportProgress(0, "Done");
        }


        // UPLOAD ONE REPLAY
        private void UploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var replayFile = (string)gvReplayList.SelectedRows[0].Cells[0].Value;

            gvReplayList.SelectedRows[0].Cells[1].Value = ProgressStage.Uploading.ToString();
            gvReplayList.SelectedRows[0].Cells[3].Value = string.Empty;

            DoUpload(replayFile, new ProcessingStage(this.OnProcessStageChange)).Forget();
        }


        // SHOW PLAYER PROFILE
        private void viewProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.PlayerID != -1)
            {
                using (Process.Start(new ProcessStartInfo("https://www.hotslogs.com/Player/Profile?PlayerID=" + Properties.Settings.Default.PlayerID))) { }
            }
            else
            {
                using (var playerIdForm = new SetPlayerID())
                {
                    playerIdForm.ShowDialog();
                }
            }
        }


        // SHOW OPTIONS FORM
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save the current auto-update setting
            var updateSetting = Properties.Settings.Default.AutoUpdate;

            // stop any running threads
            this.StopThreads();

            using (var options = new Options() { Owner = this })
            {
                options.ShowDialog();
            }

            // apply setting changes
            this.ApplySettings();

            // if auto update has been turned on, do the update check
            if (Properties.Settings.Default.AutoUpdate && updateSetting != Properties.Settings.Default.AutoUpdate)
            {
                LastUpdateCheck = DateTime.MinValue;
                this.updateWorker.RunWorkerAsync();
            }
            else if (!updateSetting)
            {
                this.updateWorker.CancelAsync();
            }
        }


        // SHOW ABOUT FORM
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var formAbout = new About())
            {
                formAbout.ShowDialog();
            }
        }


        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Focus();
        }


        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
            ShowBalloonTip("HOTS Logs", "Application is now hidden, double-click to show it.");
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Focus();
        }


        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex != -1)
                gvReplayList.Rows[e.RowIndex].Selected = true;
        }


        private void Form1_Shown(object sender, EventArgs e)
        {
#if DEBUG
            MessageBox.Show("You are running a debug version of the HOTSLogs Uploader.\n\nPlease visit www.hotslogs.com to download the latest release version.", "Debug version detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif

            if (Properties.Settings.Default.LaunchInTray)
            {
                WindowState = FormWindowState.Minimized;
                Hide();
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                hideToolStripMenuItem_Click(null, null);
            }
        }

        private void parseReplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var replayFile = (string)gvReplayList.SelectedRows[0].Cells[0].Value;

            var (_, replay) = ParseReplay(replayFile, false, ParseOptions.MinimalParsing);

            var data = new
            {
                FileName = replayFile,
                Players = replay.Players.Select(i => i.BattleNetId).ToArray(),
                replay.RandomValue,
                Fingerprint = GetReplayFingerprint(replay),
            };

            File.AppendAllText($"{Application.StartupPath}\\Fingerprints.json", JsonConvert.SerializeObject(data));
        }


        delegate void ShowUpdateMenuItemCallback();
        public void ShowUpdateMenuItem()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ShowUpdateMenuItemCallback(ShowUpdateMenuItem));
            }
            else
            {
                this.updateAvailableToolStripMenuItem.Visible = true;
            }
        }


        private async void updateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                // Check the thread hasn't been cancelled
                if (updateWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    double currentInterval = double.MaxValue;

                    switch (Properties.Settings.Default.AutoUpdateInterval)
                    {
                        case 0:
                            currentInterval = (DateTime.Now - LastUpdateCheck).TotalMinutes;
                            break;
                        case 1:
                            currentInterval = (DateTime.Now - LastUpdateCheck).TotalHours;
                            break;
                        case 2:
                            currentInterval = (DateTime.Now - LastUpdateCheck).TotalDays;
                            break;
                        case 3:
                            currentInterval = DateTime.Now.Subtract(LastUpdateCheck).Days / (365.2425 / 12);
                            break;
                        default:
                            break;
                    }


                    // Check every 4 hours
                    if (currentInterval >= Properties.Settings.Default.AutoUpdateInterval)
                    {
                        try
                        {
                            // Check for new version
                            var updateCheckResult = await this.UpdateIsAvailable();

                            // If new version available, download it
                            if (updateCheckResult.NewVersionAvailable)
                            {
                                try
                                {
                                    // if update downloads succesfully, update UI to indicate update is ready
                                    if (await this.DownloadUpdate())
                                    {
                                        this.ShowUpdateMenuItem();
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                            }

                            // Once update check is done, update last check 
                            LastUpdateCheck = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error checking or downloading update");
                        }

                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void updateAvailableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OnBeforeClose();
            this.FormClosing -= this.Form1_FormClosing;
            Application.Restart();
        }


        private void updateNotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            this.updateAvailableToolStripMenuItem_Click(sender, e);
        }


        private void bulkUploadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is IEnumerable<object> list)
            {
                foreach (var e1 in list)
                {
                    switch (e1)
                    {
                        case string s:
                            label1.Text = $"Current Operation: {s}...";
                            break;
                        case ProcessingStageEventArgs stageEventArgs:
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new ProcessingStage(OnProcessStageChange), sender, stageEventArgs);
                            }
                            else
                            {
                                this.OnProcessStageChange(sender, stageEventArgs);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            uploadProgressPanel.Visible = (e.ProgressPercentage > 0) && (e.ProgressPercentage != 100);
            progressBar1.Value = e.ProgressPercentage;
        }
    }

    public class CancelException : Exception { }
}
