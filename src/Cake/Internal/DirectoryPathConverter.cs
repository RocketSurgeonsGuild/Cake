using System;
using Cake.Core.IO;
using Newtonsoft.Json;

namespace Rocket.Surgery.Cake.Internal
{
    class DirectoryPathConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var path = (DirectoryPath)value;
            writer.WriteValue(path.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DirectoryPath);
        }
    }
}
