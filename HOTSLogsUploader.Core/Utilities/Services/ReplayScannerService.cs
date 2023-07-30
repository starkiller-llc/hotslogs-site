using HOTSLogsUploader.Core.Extensions;
using HOTSLogsUploader.Core.Models;
using HOTSLogsUploader.Core.Models.Db.Entity;
using HOTSLogsUploader.Core.Properties;
using HOTSLogsUploader.Core.Utilities.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using static Heroes.ReplayParser.DataParser;


namespace HOTSLogsUploader.Core.Utilities.Services
{
    public interface IReplayScannerService
    {
        ObservableCollection<Replay> Data { get; set; }
        string ReplayDirectory { get; set; }
    }

    public class ReplayScannerService : IReplayScannerService
    {
        private FileSystemWatcher scanner = new FileSystemWatcher();
        private ReplayDbContext dbContext;
        private readonly ICommonUploadService uploadService;
        private readonly IMatchSummaryService matchSummaryService;
        private readonly IComparer<Replay> comparer = new ReplayComparer();

        public ReplayScannerService(IServiceProvider serviceProvider, ICommonUploadService uploadService, IMatchSummaryService matchSummaryService)
        {
            InitialiseComponent();
            dbContext = serviceProvider.GetRequiredService<ReplayDbContext>();
            this.uploadService = uploadService;
            this.matchSummaryService = matchSummaryService;
        }

        private void InitialiseComponent()
        {
            scanner.EnableRaisingEvents = false;
            scanner.IncludeSubdirectories = true;
            scanner.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;
            scanner.Filter = Constants.ReplayExtensionFilter;
            scanner.Created += _scanner_Event;
            scanner.Changed += _scanner_Event;
            scanner.Deleted += _scanner_Event;
            scanner.Renamed += _scanner_Event;
        }

        public string ReplayDirectory
        {
            get => scanner.Path;
            set
            {
                if (Directory.Exists(value))
                {
                    scanner.Path = value;
                    scanner.EnableRaisingEvents = Directory.Exists(value);
                    Reload();
                }
            }
        }

        public ObservableCollection<Replay> Data { get; set; } = new ObservableCollection<Replay>();

        private void Reload()
        {
            Data.Clear();
            if (scanner.EnableRaisingEvents)
            {
                HotsLogsExtensions.GetReplaysInDirectory(scanner.Path, Constants.ReplayExtensionFilter)
                    .Where(i => Path.GetDirectoryName(i).EndsWith("Replays\\Multiplayer"))
                    .ToList()
                    .ForEach(replayPath => Data.AddSorted(dbContext.Replays.Find(replayPath) ?? new Replay { FilePath = replayPath }, comparer));
            }

            UploadExistingReplays();
        }

        private void UploadExistingReplays()
        {
            if (Settings.Default.AutomaticUpload)
            {
                var unUploadedReplays = Data.Where(i => !i.DateUploaded.HasValue || i.UploadStatus == ReplayParseResult.Exception.ToString()).ToList();
                unUploadedReplays.ForEach(r => uploadService.AddToQueue(new UploadJob { Replay = r }));
            }
        }

        private void _scanner_Event(object sender, FileSystemEventArgs e)
        {
            lock (Data)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                        case WatcherChangeTypes.Changed:
                            var replay = Data.All(i => i.FilePath != e.FullPath)
                                ? new Replay { FilePath = e.FullPath }
                                : null;
                            if (replay != null)
                            {
                                replay.Parse();
                                Data.AddSorted(replay, comparer);
                                if (Settings.Default.AutomaticUpload)
                                {
                                    var job = new UploadJob { Replay = replay };
                                    if (Settings.Default.ShowMatchSummary)
                                    {
                                        job.OnUploadSuccess = matchSummaryService.ShowMatchSummaryAction;
                                    }

                                    uploadService.AddToQueue(job);
                                }
                            }

                            break;
                        case WatcherChangeTypes.Deleted:
                            var deletedItem = Data.SingleOrDefault(i => i.FilePath == e.FullPath);
                            if (deletedItem != null)
                            {
                                Data.Remove(deletedItem);
                            }

                            break;
                        case WatcherChangeTypes.Renamed:
                            var renamedItem =
                                Data.SingleOrDefault(i => i.FilePath == ((RenamedEventArgs)e).OldFullPath) ??
                                new Replay { FilePath = ((RenamedEventArgs)e).OldFullPath };
                            renamedItem.FilePath = ((RenamedEventArgs)e).FullPath;
                            break;
                        case WatcherChangeTypes.All:
                        default:
                            break;
                    }
                });
            }            
        }
    }

    internal class ReplayComparer : IComparer<Replay>
    {
        /// <summary>
        /// Compares date created and returns descending order
        /// </summary>
        public int Compare(Replay x, Replay y)
        {
            if (x.DateCreated == y.DateCreated) return 0;
            if (x.DateCreated < y.DateCreated) return 1;
            return -1;
        }
    }
}
