using HotsLogsApi.MigrationControllers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Linq;
using System.Text.Json;

namespace HotsLogsApi;

public class MyJsonOutputFormatter : SystemTextJsonOutputFormatter
{
    public MyJsonOutputFormatter([NotNull] JsonSerializerOptions jsonSerializerOptions) :
        base(jsonSerializerOptions) { }

    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        var g = context.HttpContext.GetEndpoint();
        return g?.Metadata.OfType<MigrationAttribute>().Any() ?? false;
    }
}
