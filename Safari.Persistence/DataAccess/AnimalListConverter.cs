using Safari.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Safari.Persistence.DataAccess {
    public class AnimalListConverter : JsonConverter<List<Animal>> {
        public override List<Animal> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) {
            var animals = new List<Animal>();

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.StartObject) {
                    var animal = JsonSerializer.Deserialize<Animal>(ref reader, options);
                    if (animal != null) animals.Add(animal);
                }
            }

            return animals;
        }

        public override void Write(
            Utf8JsonWriter writer,
            List<Animal> value,
            JsonSerializerOptions options) {
            writer.WriteStartArray();
            foreach (var animal in value) {
                JsonSerializer.Serialize(writer, animal, options);
            }
            writer.WriteEndArray();
        }
    }
}
