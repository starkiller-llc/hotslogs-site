using Heroes.ReplayParser;
using static Heroes.ReplayParser.DataParser;
using System;
using System.IO;
using System.Threading;
using Serilog;

namespace HOTSLogsUploader.Utilities
{
    public class ReplayScanner : FileSystemWatcher
    {
        public ReplayScanner()
            : base()
        {
            this.IncludeSubdirectories = true;
            this.EnableRaisingEvents = false;
            this.UploadReplaysAutomatically = false;
            this.ShowMatchSummary = false;
            this.Filter = Constants.ReplayExtensionFilter;
            this.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName;
            this.Changed += new FileSystemEventHandler(this.ReplayScanner_OnChange);
            this.Created += new FileSystemEventHandler(this.ReplayScanner_OnChange);
        }


        public bool UploadReplaysAutomatically { get; set; }
        public bool ShowMatchSummary { get; set; }

        public event ProcessingStage ProcessingStage;

        private void ReplayScanner_OnChange(object sender, FileSystemEventArgs e)
        {

            try
            {
                // Wait for the file to become available
                this.WaitForFileAccess(e.FullPath);

                // Parse the replay
                var (replayParseResult, replay) = ParseReplay(e.FullPath, false, ParseOptions.MinimalParsing);

                // Replay must parse successfully to avoid attempting to upload incomplete/vs AI/Try Me!/etc replays
                if (replayParseResult == ReplayParseResult.Success)
                {
                    if (e.ChangeType == WatcherChangeTypes.Created)
                    {
                        ProcessingStage?.Invoke(sender, new ProcessingStageEventArgs(e.FullPath, ProgressStage.Added));
                    }
                    
                    // Always add to list
                    if (this.UploadReplaysAutomatically)
                    {
                        // If we already have the replay, don't upload it again and just return success
                        if (ReplayUploader.ReplayNeedsUploading(replay))
                        {
                            // Upload replay to hotslogs
                            new Thread(async () =>
                            {
                                await ReplayUploader.DoUpload(e.FullPath, ProcessingStage, this.ShowMatchSummary);
                            })
                            { IsBackground = true }.Start();
                        }
                        else
                        {
                            ProcessingStage?.Invoke(sender, new ProcessingStageEventArgs(e.FullPath, ProgressStage.Completed, DateTime.UtcNow, ReplayParseResult.Success));
                            // Will add functionality later to show match summary here   
                        }

                        // Also upload to heroesprofile
                        if (Properties.Settings.Default.SendtoHP)
                        {
                            new Thread(async () =>
                            {
                                await ReplayUploader.SendToHeroesProfiles(e.FullPath);
                            })
                            { IsBackground = true }.Start();
                        }
                    }
                }
                else
                {
                    // If the replay didn't parse successfully, don't upload but still add notification
                    ProcessingStage?.Invoke(sender, new ProcessingStageEventArgs(e.FullPath, ProgressStage.Completed, DateTime.UtcNow, replayParseResult));
                    Log.Error($"File {e.FullPath} failed to parse successfully. Parse result \"{replayParseResult}\"");
                }
            }
            catch (Exception ex)
            {
                // Log an e
                Log.Error(ex, "Error checking for updates");
                ProcessingStage?.Invoke(sender, new ProcessingStageEventArgs(e.FullPath, ProgressStage.Completed, DateTime.UtcNow, ReplayParseResult.Exception));
            }
        }


        private void WaitForFileAccess(string filename)
        {
            var startTime = DateTime.Now;

            try
            {
                while (true)
                {
                    // Keep retrying 
                    if ((DateTime.Now - startTime).TotalMilliseconds > Constants.MaximumWaitTime)
                    {
                        throw new Exception("File cannot be opened for reading.");
                    }

                    // try to open the file, otherwise wait for 50 MS
                    try
                    {
                        using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            break;
                        }
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Time out waiting for file access");
                throw;
            }
            
        }        
    }
}
