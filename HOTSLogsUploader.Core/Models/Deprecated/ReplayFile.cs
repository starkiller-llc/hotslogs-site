using HOTSLogsUploader.Common;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using HOTSLogsUploader.Core.Extensions;
using static Heroes.ReplayParser.DataParser;
using System.Collections.Generic;

namespace HOTSLogsUploader.Core.Models.Deprecated
{
    public class ReplayFile : INotifyPropertyChanged
    {
        private string _uploadStatus;
        private string _filePath;
        private ReplayParseResult? _parseResult;
        private DateTime? _uploadDate;
        private Guid? _fingerprint;
        private string _dateUploaded;
        private DateTime _createdDate;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReplayFile()
        {
        }


        public ReplayFile(ReplayFile i)
            : this(i.FilePath)
        {
            this.Update(i);
        }


        public ReplayFile(string filePath)
        {
            this.FilePath = filePath;
            _createdDate = filePath.GetFileDateTime();
        }


        [JsonConstructor]
        public ReplayFile(string filepath, string dateUploaded, string uploadStatus)
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


        public void Update(ReplayFile source)
        {
            if (source != null)
            {
                this.UploadTimestamp = source.UploadTimestamp;
                this.UploadStatus = source.UploadStatus;
                this.Fingerprint = source.Fingerprint;
                this._createdDate = source.DateCreated;
            }
        }


        public void Update(UploadResponse source)
        {
            this.UploadTimestamp = DateTime.UtcNow;

            if (source != null)
            {
                this.ParseResult = (ReplayParseResult)source.Result;
            }
            else
            {
                this.ParseResult = ReplayParseResult.Exception;
            }
        }


        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }


        [JsonIgnore]
        public string FileName => Path.GetFileNameWithoutExtension(this.FilePath);


        [JsonProperty(PropertyName = "DateUploaded")]
        public DateTime? UploadTimestamp
        {
            get => _uploadDate;
            set
            {
                if (_uploadDate == value)
                {
                    return;
                }

                _uploadDate = value;
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

        [JsonIgnore]
        public DateTime DateCreated => _createdDate;

        public string UploadStatus
        {
            get
            {
                return this._uploadStatus;
            }

            set
            {
                if (_uploadStatus == value)
                {
                    return;
                }

                _uploadStatus = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public ReplayParseResult? ParseResult
        {
            get => _parseResult;
            set
            {
                if (_parseResult == value)
                {
                    return;
                }
                _parseResult = value;
                OnPropertyChanged();
                if (_parseResult.HasValue)
                {
                    UploadStatus = _parseResult?.AsString();
                }
            }
        }


        public Guid? Fingerprint
        {
            get => _fingerprint;
            set
            {
                _fingerprint = value;
                OnPropertyChanged();
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ReplayFileCollection
    {
        public ReplayFileCollection()
        {
            Data = new List<ReplayFile>();
        }

        public ReplayFileCollection(IEnumerable<ReplayFile> data)
            : this()
        {
            Data.AddRange(data);
        }

        public List<ReplayFile> Data { get; set; }
    }
}
