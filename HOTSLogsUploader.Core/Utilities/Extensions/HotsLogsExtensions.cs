using HOTSLogsUploader.Core.Enums;
using HOTSLogsUploader.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Heroes.ReplayParser.DataParser;

namespace HOTSLogsUploader.Core.Extensions
{
    public static class HotsLogsExtensions
    {
        public static string GetDescription<T>(this T value) where T : struct, IConvertible
        {
            var description = value.ToString();
            var fieldInfo = value.GetType().GetField(description);

            return fieldInfo.GetCustomAttributes(true).OfType<DescriptionAttribute>().FirstOrDefault()?.Description ?? description;
        }

        public static T ToEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value);
            }
            catch (Exception)
            {
                return default;
            }
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
                    return DateTime.ParseExact(r.Match(file.Name).Groups[1].Value, Constants.ReplayDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
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

        public static IEnumerable<string> GetReplaysInDirectory(string folder, string filter)
        {
            string[] found = null;
            try
            {
                found = Directory.GetFiles(folder, filter);
            }
            catch { }

            if (found != null)
                foreach (var item in found)
                    yield return item;
            found = null;
            try
            {
                found = Directory.GetDirectories(folder);
            }
            catch { }
            if (found != null)
                foreach (var item in found)
                    foreach (var subItem in GetReplaysInDirectory(item, filter))
                        yield return subItem;
        }


        public static System.Drawing.Color ToColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }      


        public static void Forget(this Task task) { }


        public static string AsString(this ProgressStage stage)
        {
            if (stage == ProgressStage.Uploading)
            {
                return "Uploading...";
            }
            else
            {
                return string.Empty;
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

        public static void SaveOrUpdate(this DbContext context, object entity)
        {
            lock(context)
            {
                if (context.Entry(entity).State == EntityState.Detached)
                {
                    context.Add(entity);
                }
                else
                {
                    context.Update(entity);
                }

                context.SaveChanges();
            }
        }
    }
}
