using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static Heroes.ReplayParser.DataParser;

namespace HOTSLogsUploader.Utilities
{
    public class ReplayDataSource
    {
        public ReplayDataSource()
        {
        }


        public ReplayDataSource(bool autoLoad)
            : this()
        {
            if (autoLoad)
            {
                var dataSource = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ReplayData.json");

                if (!File.Exists(dataSource))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dataSource));
                    File.Create(dataSource).Close();
                }

                this.Load(true);
            }
        }


        public List<ReplayDataItem> Data { get; set; } = new List<ReplayDataItem>();


        public void Save()
        {
            File.WriteAllText(
                Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ReplayData.json"),
                JsonConvert.SerializeObject(this));
        }


        public void Load(bool saveAfterLoad = false)
        {
            var previousReplays = JsonConvert.DeserializeObject<ReplayDataSource>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ReplayData.json")));

            if (previousReplays != null)
            {
                // add excluding duplicate files - duplicates could be added in previous versions
                previousReplays.Data.ForEach(i =>
                {
                    if (!this.Data.Any(j => j.FilePath == i.FilePath))
                    {
                        this.Data.Add(new ReplayDataItem(i));
                    }
                });                
            }
            
            if (saveAfterLoad)
            {
                this.Save();
            }
        }
    }


    public class ReplayDataItem : INotifyPropertyChanged
    {
        private string uploadStatus;
        private string filePath;
        private ReplayParseResult? parseResult;
        private DateTime? uploadDate;
        private Guid? fingerprint;
        private string _dateUploaded;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReplayDataItem()
        {
        }


        public ReplayDataItem(ReplayDataItem i)
            : this(i.FilePath)
        {
            this.Update(i);
        }


        public ReplayDataItem(string filePath)
        {
            this.FilePath = filePath;
        }


        [JsonConstructor]
        public ReplayDataItem(string filepath, string dateUploaded, string uploadStatus)
        {
            this.FilePath = filepath;
            this.UploadStatus = uploadStatus;

            // try to parse the date
            try
            {
                this.UploadTimestamp = DateTime.Parse(dateUploaded);
            }
            catch
            {
                this.UploadTimestamp = null;
            }
        }


        public void Update(ReplayDataItem source)
        {
            if (source != null)
            {
                this.UploadTimestamp = source.UploadTimestamp;
                this.UploadStatus = source.UploadStatus;
                this.Fingerprint = source.Fingerprint;
            }
        }


        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }


        [JsonIgnore]
        public string FileName => Path.GetFileNameWithoutExtension(this.FilePath);


        [JsonProperty(PropertyName = "DateUploaded")]
        public DateTime? UploadTimestamp
        {
            get => uploadDate;
            set
            {
                if (uploadDate == value)
                {
                    return;
                }

                uploadDate = value;
                OnPropertyChanged();
                this.DateUploaded = this.UploadTimestamp?.ToLocalTime().ToString("G", CultureInfo.CurrentCulture) ?? string.Empty;
            }
        }


        [JsonIgnore]
        public string DateUploaded
        {
            get => _dateUploaded;
            set
            {
                if (_dateUploaded == value)
                {
                    return;
                }
                _dateUploaded = value;
                OnPropertyChanged();
            }
        }


        public string UploadStatus
        {
            get
            {
                return this.uploadStatus;
            }

            set
            {
                if (uploadStatus == value)
                {
                    return;
                }

                uploadStatus = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public ReplayParseResult? ParseResult
        {
            get => parseResult;
            set
            {
                if (parseResult == value)
                {
                    return;
                }
                parseResult = value;
                OnPropertyChanged();
                if (parseResult.HasValue)
                {
                    UploadStatus = parseResult?.AsString();
                }
            }
        }


        public Guid? Fingerprint 
        { 
            get => fingerprint;
            set
            {
                fingerprint = value;
                OnPropertyChanged();
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
