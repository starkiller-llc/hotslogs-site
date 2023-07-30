using System;
using System.IO;
using System.Threading.Tasks;
using static Heroes.ReplayParser.DataParser;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace HOTSLogsUploader
{
    public delegate void ProcessingStage(object sender, ProcessingStageEventArgs e);

    public class ProcessingStageEventArgs : EventArgs
    {
        public ProcessingStageEventArgs(string filePath, ProgressStage stage)
        {
            this.FilePath = filePath;
            this.Stage = stage;
        }

        public ProcessingStageEventArgs(string filePath, ProgressStage stage, DateTime uploadDate, ReplayParseResult replayParseResult)
            : this(filePath, stage)
        {
            this.UploadDate = uploadDate;
            this.ReplayParseResult = replayParseResult;
        }

        public ProcessingStageEventArgs(string filePath, ProgressStage stage, DateTime uploadDate, ReplayParseResult replayParseResult, Guid? fingerprint)
            : this(filePath, stage, uploadDate, replayParseResult)
        {
            this.UploadDate = uploadDate;
            this.ReplayParseResult = replayParseResult;
            this.Fingerprint = fingerprint;
        }

        public string FilePath { get; set; }
        public ProgressStage Stage { get; set; }
        public DateTime? UploadDate { get; set; }
        public ReplayParseResult? ReplayParseResult { get; set; }
        public Guid? Fingerprint { get; set; }
    }


    public sealed class Counter
    {
        private int counter = 0;
        public int WorkCount { get; set; }

        public void Reset()
        {
            lock (this)
            {
                counter = 0;
                WorkCount = 0;
            }
        }

        public int Value()
        {
            lock (this)
            {
                return counter;
            }
        }

        private void Add(int i = 1)
        {
            lock (this)
            {
                counter += i;
            }
        }

        public int Percentage()
        {
            lock (this)
            {
                var result = (int)((float)counter / (float)WorkCount * 100);
                return Math.Min(result, 100);
            }
        }

        public static Counter operator ++(Counter a)
        {
            lock (a)
            {
                a.Add();
                return a;
            }
        }

        public static implicit operator int(Counter a)
        {
            lock (a)
            {
                return a.Value();
            }
        }
    }


    public static class Constants
    {
#if DEBUG
        public const string hotslogsUrl = @"https://dev.hotslogs.com";
#else
        public const string hotslogsUrl = @"https://www.hotslogs.com";
#endif
        public const string heroesProfilesUploadEndpoint = @"https://api.heroesprofile.com/api/upload/alt";

        public const int MaximumWaitTime = 30000;
        public const int MaximumFileSize = 20000000;
        public const int FingerprintQueryPageSize = 20;
        public const int FileUploadPageSize = 10;

        public const string ReplayExtensionFilter = "*.StormReplay";
        public const string UpdateFileName = "update.exe";
        public const string BackupInstanceExt = ".bak";
    }


    public enum ProgressStage
    {
        None,
        Added,
        Uploading,
        Completed,
    }


    public static class PagingExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source.Count() == 0;
        }


        public static IEnumerable<IEnumerable<T>> Pages<T>(this IEnumerable<T> source, int pageSize)
        {
            Contract.Requires(source != null);
            Contract.Requires(!source.IsEmpty());
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var currentPage = new List<T>(pageSize)
                    {
                        enumerator.Current
                    };

                    while (currentPage.Count < pageSize && enumerator.MoveNext())
                    {
                        currentPage.Add(enumerator.Current);
                    }

                    yield return new ReadOnlyCollection<T>(currentPage);
                }
            }
        }
    }


    public static class HotsLogsExtensions
    {
        public static string ToString(this ReplayParseResult value)
        {
            return Enum.GetName(value.GetType(), value);
        }


        public static DateTime GetFileDateTime(this string value)
        {
            if (File.Exists(value))
            {
                FileInfo file = new FileInfo(value);

                Regex r = new Regex(@"^(\d{4}\-\d{2}\-\d{2}\s\d{2}\.\d{2}\.\d{2}).*?\.StormReplay", RegexOptions.IgnoreCase);
                // Use the date in the file name if the replay hasn't been renamed or has a date in the correct format
                if (r.IsMatch(file.Name))
                {
                    return DateTime.ParseExact(r.Match(file.Name).Groups[1].Value, replayDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                }
                else
                {
                    return file.CreationTime;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }


        static readonly string[] replayDateFormat =
        {
            "yyyy-MM-dd HH.mm.ss"
        };


        public static ReplayParseResult ToReplayParseResult(this string value)
        {
            return (ReplayParseResult)Enum.Parse(typeof(ReplayParseResult), value);
        }


        public static void Forget(this Task task) { }


        public static string AsString(this ProgressStage stage)
        {
            switch (stage)
            {
                case ProgressStage.Uploading:
                    return "Uploading...";
                case ProgressStage.Completed:
                    return "Success";
                case ProgressStage.None:
                case ProgressStage.Added:
                default:
                    return "";
            }
        }


        public static string AsString(this ReplayParseResult value)
        {
            switch (value)
            {
                case ReplayParseResult.Success:
                case ReplayParseResult.Incomplete:
                case ReplayParseResult.Duplicate:
                case ReplayParseResult.Exception:
                    return value.ToString();
                case ReplayParseResult.ComputerPlayerFound:
                    return "AI Player Found";
                case ReplayParseResult.TryMeMode:
                    return "Try Mode";
                case ReplayParseResult.UnexpectedResult:
                    return "Unexpected Result";
                case ReplayParseResult.FileNotFound:
                    return "File Not Found";
                case ReplayParseResult.PreAlphaWipe:
                    return "Pre-Alpha Wipe";
                case ReplayParseResult.FileSizeTooLarge:
                    return "File Size Too Large";
                case ReplayParseResult.PTRRegion:
                    return "PTR Replay";
                default:
                    return string.Empty;
            }
        }
    }
}
