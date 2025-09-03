
using Safari.Persistence.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Safari.Persistence.DataAccess {
    public class DataAccess : IDataAccess {
        public GameTable Load(String path) {
            if (!File.Exists(path)) {
                throw new Exception("File does not exist");
            }

            var options = new JsonSerializerOptions {
                Converters = {
                    new TileArrayConverter(),
                    new TileConverter(),
                    new AnimalConverter(),
                    new TupleConverter(),
                    new AnimalListConverter()
                }
            };

            using (FileStream jsonStream = File.OpenRead(path)) {
                return JsonSerializer.Deserialize<GameTable>(jsonStream, options) ?? new GameTable();
            }
        }
        
        public void Save(GameTable gameTable, String path) {
            var options = new JsonSerializerOptions {
                Converters = {
                    new TileArrayConverter(),
                    new TileConverter(),
                    new AnimalConverter(),
                    new TupleConverter(),
                    new AnimalListConverter()
                },
                WriteIndented = true
            };

            String jsonString = JsonSerializer.Serialize(gameTable, options);
            if(File.Exists(path)) {
                File.Delete(path);
            }
            File.WriteAllText(path, jsonString);
        }
    }
}
