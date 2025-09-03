using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Safari.Persistence.Tiles;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Safari.Persistence {

    public class TileConverter : JsonConverter<Tile> {
        // Map JSON type discriminators to Tile subtypes
        private static readonly Dictionary<string, Type> _typeMapping = new()
        {
        { "bush", typeof(Bush) },
        { "empty", typeof(Empty) },
        { "grass", typeof(Grass) },
        { "hill", typeof(Hill) },
        { "lake", typeof(Lake) },
        { "river", typeof(River) },
        { "road", typeof(Road) },
        { "tree", typeof(Tree) }
    };

        public override Tile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Utf8JsonReader readerClone = reader; // Clone the reader to inspect properties
            string? typeDiscriminator = null;

            // Look for the "$type" property first
            if (readerClone.TokenType == JsonTokenType.StartObject) {
                readerClone.Read();
                while (readerClone.TokenType == JsonTokenType.PropertyName) {
                    string propName = readerClone.GetString() ?? "";
                    if (propName == "$type") {
                        readerClone.Read();
                        typeDiscriminator = readerClone.GetString();
                        break;
                    }
                    readerClone.Skip();
                    readerClone.Read();
                }
            }

            // Determine the concrete Tile type
            if (typeDiscriminator == null || !_typeMapping.TryGetValue(typeDiscriminator, out Type? tileType)) {
                throw new JsonException($"Invalid or missing type discriminator for Tile.");
            }

            return (Tile)(JsonSerializer.Deserialize(ref reader, tileType, options) ?? new Empty((0,0)));
        }

        public override void Write(Utf8JsonWriter writer, Tile tile, JsonSerializerOptions options) {
            writer.WriteStartObject();

            // Add the type discriminator
            string typeName = tile switch {
                Bush _ => "bush",
                Empty _ => "empty",
                Grass _ => "grass",
                Hill _ => "hill",
                Lake _ => "lake",
                River _ => "river",
                Road _ => "road",
                Tree _ => "tree",
                _ => throw new JsonException($"Unknown Tile type: {tile.GetType().Name}")
            };
            writer.WriteString("$type", typeName);

            // Serialize common properties
            writer.WriteBoolean("canSee", tile.canSee);
            writer.WritePropertyName("position");
            writer.WriteStartArray();
            writer.WriteNumberValue(tile.position.Item1);
            writer.WriteNumberValue(tile.position.Item2);
            writer.WriteEndArray();

            // // Serialize additional properties specific to the derived class
            // switch (tile) {
            //     // Example for Bush (add more cases as needed)
            //     case Bush bush:
            //         writer.WriteString("bushType", bush.BushType);
            //         break;
            //         // Add other cases for Grass, Lake, etc., if they have extra properties
            // }

            writer.WriteEndObject();
        }
    }
}
