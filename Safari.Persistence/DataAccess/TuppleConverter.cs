using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Safari.Persistence.DataAccess {
    public class TupleConverter : JsonConverter<(int, int)> {
        public override (int, int) Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected array start");

            reader.Read();
            var item1 = reader.GetInt32();

            reader.Read();
            var item2 = reader.GetInt32();

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("Expected array end");

            return (item1, item2);
        }

        public override void Write(
            Utf8JsonWriter writer,
            (int, int) value,
            JsonSerializerOptions options) {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Item1);
            writer.WriteNumberValue(value.Item2);
            writer.WriteEndArray();
        }
    }
}
