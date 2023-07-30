using HOTSLogsUploader.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Heroes.ReplayParser.DataParser;

namespace HOTSLogsUploader.Core.Models.Db.Entity
{
    public class Replay : INotifyPropertyChanged
    {
        private int? id;
        private Guid? fingerprint;
        private string filePath;
        private DateTime? dateUploaded;
        private string uploadStatus;
        private ReplayParseResult? parseResult;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }
        public Guid? Fingerprint
        {
            get => fingerprint;
            set
            {
                fingerprint = value; ;
                OnPropertyChanged();
            }
        }
        [Key]
        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value; ;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public string FileName => Path.GetFileNameWithoutExtension(FilePath);
        public DateTime? DateUploaded
        {
            get => dateUploaded;
            set
            {
                dateUploaded = value; ;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public DateTime? DateUploadedLocal => DateUploaded?.ToLocalTime();
        [JsonIgnore]
        public DateTime DateCreated => FilePath.GetFileDateTime();
        public string UploadStatus
        {
            get => uploadStatus;
            set
            {
                uploadStatus = value; ;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public ReplayParseResult? ParseResult
        {
            get => parseResult;
            set
            {
                parseResult = value; ;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
