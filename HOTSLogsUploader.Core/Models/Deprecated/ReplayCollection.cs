using HOTSLogsUploader.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Models.Deprecated
{
    public class ReplayCollection
    {
        private ReplayFileCollection _uploadHistory = new ReplayFileCollection();
        private FileSystemWatcher _scanner = new FileSystemWatcher();

        public ReplayCollection()
        {
            _scanner.EnableRaisingEvents = false;
            _scanner.IncludeSubdirectories = true;
            _scanner.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;
            _scanner.Filter = Constants.ReplayExtensionFilter;
            _scanner.Created += _scanner_Event;
            _scanner.Changed += _scanner_Event;
            _scanner.Deleted += _scanner_Event;
            _scanner.Renamed += _scanner_Event;

            var history = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReplayData.json");            
            if (File.Exists(history))
            {
                var _uploadHistory = JsonSerializer.Deserialize<ReplayFileCollection>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReplayData.json")));
            }            
        }

        public string ReplayDirectory
        {
            get => _scanner.Path;
            set
            {
                if (Directory.Exists(value))
                {
                    _scanner.Path = value;
                    _scanner.EnableRaisingEvents = Directory.Exists(value);
                    Reload();
                }
            }
        }

        public ObservableCollection<ReplayFile> Data { get; set; } = new ObservableCollection<ReplayFile>();

        private void Reload()
        {
            Data.Clear();

            if (_scanner.EnableRaisingEvents)
            {
                foreach (var replayPath in HotsLogsExtensions.GetReplaysInDirectory(_scanner.Path, Constants.ReplayExtensionFilter)
                    .Where(i => Path.GetDirectoryName(i).EndsWith("Replays\\Multiplayer")))
                    //.OrderByDescending(i => i.GetFileDateTime()))
                {
                    var result = new ReplayFile(replayPath);

                    result.UploadTimestamp = _uploadHistory.Data.FirstOrDefault(i => i.FilePath == result.FilePath)?.UploadTimestamp;
                    result.UploadStatus = _uploadHistory.Data.FirstOrDefault(i => i.FilePath == result.FilePath)?.UploadStatus;

                    Data.Add(result);
                }
            }
        }

        private void _scanner_Event(object sender, FileSystemEventArgs e)
        {
            lock (Data)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                        case WatcherChangeTypes.Changed:
                            if (Data.All(i => i.FilePath != e.FullPath))
                            {
                                Data.Add(new ReplayFile(e.FullPath));
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
                            var renamedItem = Data.SingleOrDefault(i => i.FilePath == ((RenamedEventArgs)e).OldFullPath) ?? new ReplayFile(((RenamedEventArgs)e).OldFullPath);
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
}
