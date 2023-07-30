using HelperCore;
using HotsLogsApi.BL.Migration;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class DataController : ControllerBase
{
    [HttpGet("{id}")]
    public object GetDataByString(string id)
    {
        if (id == "Maps")
        {
            return Global.GetLocalizationAlias().Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map)
                .Select(
                    i => new
                    {
                        i.PrimaryName,
                        ImageURL = i.PrimaryName.PrepareForImageURL(),
                        Translations = i.AliasesCsv,
                    }).OrderBy(i => i.PrimaryName);
        }

        if (id == "Heroes")
        {
            return Global.GetLocalizationAlias().Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
                .Select(
                    i => new
                    {
                        i.PrimaryName,
                        ImageURL = i.PrimaryName.PrepareForImageURL(),
                        i.AttributeName,
                        Group = i.NewGroup,
                        Translations = i.AliasesCsv,
                    }).OrderBy(i => i.PrimaryName);
        }

        if (id != null && id.StartsWith("UploaderVersion"))
        {
            return UploaderVersionHelper.Version.FileVersion;
        }

        return null;
    }
}
