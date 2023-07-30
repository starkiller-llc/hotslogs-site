using HOTSLogsUploader.Core.Models.Db.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Models
{
    public class ReplayDbContext : DbContext
    {
        private static string _fileName { get; set; }

        public ReplayDbContext(DbContextOptions<ReplayDbContext> options) : base(options)
        {
        }

        public string FileName
        {
            get => _fileName;
            set => _fileName = value;
        }

        public DbSet<Replay> Replays { get; set; }

        public void LoadFromFile()
        {
            if (!Replays.Any() && File.Exists(_fileName))
            {
                Replays.AddRange(JsonSerializer.Deserialize<IList<Replay>>(File.ReadAllText(_fileName)));
                SaveChanges();
            }
        }

        public void SaveToFile()
        {
            File.WriteAllText(_fileName, JsonSerializer.Serialize(Replays.ToList()));
        }
    }
}
