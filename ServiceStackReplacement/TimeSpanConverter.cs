using Newtonsoft.Json;
using System;
using System.Xml;

namespace ServiceStackReplacement;

public class TimeSpanConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        return XmlConvert.ToTimeSpan(value);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var ts = (TimeSpan)value;
        var tsString = XmlConvert.ToString(ts);
        serializer.Serialize(writer, tsString);
    }
}
