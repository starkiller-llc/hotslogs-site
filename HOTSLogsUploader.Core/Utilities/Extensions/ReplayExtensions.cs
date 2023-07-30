using Heroes.ReplayParser;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static Heroes.ReplayParser.DataParser;
using Replay = Heroes.ReplayParser.Replay;
using ReplayEntity = HOTSLogsUploader.Core.Models.Db.Entity.Replay;

namespace HOTSLogsUploader.Core.Utilities.Extensions
{
    public static class ReplayExtensions
    {
        public static ReplayParseResult Parse(this ReplayEntity replayEntity)
        {
            try
            {
                var (parseResult, replay) = ParseReplay(replayEntity.FilePath, false, ParseOptions.MinimalParsing);
                replayEntity.ParseResult = parseResult;

                if (parseResult == ReplayParseResult.Success)
                {
                    replayEntity.Fingerprint = replay.GetFingerprint();
                }
                else
                {
                    replayEntity.Id = -replayEntity.GetHashCode();
                }
            }
            catch (Exception)
            {
                replayEntity.ParseResult = ReplayParseResult.Exception;
                replayEntity.Fingerprint = null;
            }

            return replayEntity.ParseResult.Value;
        }

        public static Guid? GetFingerprint(this Replay replay)
        {
            try
            {
                using var md5 = MD5.Create();
                StringBuilder builder = new();

                replay.Players
                    .Select(i => i.BattleNetId)
                    .OrderBy(i => i)
                    .ToList()
                    .ForEach(i => builder.Append(i.ToString()));

                builder.Append(replay.RandomValue);

                return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString())));
            }
            catch
            {
                return null;
            }
        }

        public static NameValueCollection GetUploadParameters(this ReplayEntity replayEntity)
        {
            var uri = HttpUtility.ParseQueryString(string.Empty);
            uri.Add("filename", replayEntity.FileName);
            uri.Add("uploaderVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            return uri;
        }
    }
}
