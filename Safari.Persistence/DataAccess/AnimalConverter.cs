using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Safari.Persistence.Entities;

namespace Safari.Persistence.DataAccess {
    public class AnimalConverter : JsonConverter<Animal> {
        public override Animal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
            Utf8JsonReader readerClone = reader;
            using JsonDocument doc = JsonDocument.ParseValue(ref readerClone);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("Type", out JsonElement typeElement))
                throw new JsonException("Missing 'Type' discriminator");

            string typeName = typeElement.GetString() ?? "";

            JsonDocument.ParseValue(ref reader);

            return typeName switch {
                "Antilope" => DeserializeAnimal<Antilope>(root, options),
                "Lion" => DeserializeAnimal<Lion>(root, options),
                "Elephant" => DeserializeAnimal<Elephant>(root, options),
                "Hyena" => DeserializeAnimal<Hyena>(root, options),
                _ => throw new JsonException($"Unknown type: {typeName}")
            };
        }

        private T DeserializeAnimal<T>(JsonElement element, JsonSerializerOptions options)
            where T : Animal {
            string rawJson = element.GetRawText();
            var result = JsonSerializer.Deserialize<T>(rawJson, options);
            if (result == null) throw new Exception("des res was null");
            return result;
        }

        public override void Write(
        Utf8JsonWriter writer,
        Animal value,
        JsonSerializerOptions options) {
            writer.WriteStartObject();
            writer.WriteString("Type", value.GetType().Name);

            // Serialize all properties of the concrete type
            foreach (var prop in value.GetType().GetProperties()
                     .Where(p => p.Name != "Type" && p.CanRead)) {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, prop.GetValue(value), options);
            }

            writer.WriteEndObject();
        }
    }
}
