using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;
using UnityWebBrowser.Shared;

namespace UnityWebBrowser.Logging
{
    [Preserve]
    internal class JsonLogSeverityConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value is string value)
            {
                if (value == "Error")
                    return LogSeverity.Error;
                if (value == "Warning")
                    return LogSeverity.Warn;
                if (value == "Debug")
                    return LogSeverity.Debug;
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(string))
                return true;

            return false;
        }
    }
}