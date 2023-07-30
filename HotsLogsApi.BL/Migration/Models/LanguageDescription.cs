using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Models;

public class LanguageDescription
{
    public string LanguageCode { get; set; }
    public Dictionary<string, string> Strings { get; set; }
}
