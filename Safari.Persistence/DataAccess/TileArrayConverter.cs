using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.DataAccess {
    using Safari.Persistence.Tiles;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class TileArrayConverter : JsonConverter<Tile[,]> {
        public override Tile[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            int rows = 0, columns = 0;
            List<Tile> data = new List<Tile>();

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName) {
                    string propName = reader.GetString() ?? "";
                    reader.Read();
                    switch (propName) {
                        case "Rows":
                            rows = reader.GetInt32();
                            break;
                        case "Columns":
                            columns = reader.GetInt32();
                            break;
                        case "Data":
                            reader.Read(); // StartArray
                            while (reader.TokenType != JsonTokenType.EndArray) {
                                var tile = JsonSerializer.Deserialize<Tile>(ref reader, options);
                                data.Add(tile ?? new Empty((0,0)));
                                reader.Read();
                            }
                            break;
                    }
                }
            }

            Tile[,] result = new Tile[rows, columns];
            int index = 0;
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) {
                    result[i, j] = data[index++];
                }
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Tile[,] value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            writer.WriteNumber("Rows", value.GetLength(0));
            writer.WriteNumber("Columns", value.GetLength(1));
            writer.WritePropertyName("Data");
            writer.WriteStartArray();
            foreach (var tile in value) {
                JsonSerializer.Serialize(writer, tile, options);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
