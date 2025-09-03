using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Safari.Persistence.DataAccess;
using Safari.Persistence.Entities;
using Safari.Persistence.Tiles;

namespace Safari.Persistence {
    public class GameTable {
        [JsonNumberHandling(JsonNumberHandling.Strict)]
        #region Properties
        public int money { get; set; }
        public int tileWidth { get; set; }
        public int tileHeight { get; set; }
        public int visitors { get; set; }
        public double speed { get; set; }
        public int days { get; set; }
        public int difficulty { get; set; }
        public int satisfaction { get; set; }
        [JsonConverter(typeof(AnimalListConverter))]
        public List<Animal> animals { get; set; } = new();
        public List<Ranger> rangers { get; set; } = new();
        public List<Poacher> poachers { get; set; } = new();
        public List<Jeep> vehicles { get; set; } = new();
        [JsonConverter(typeof(TileArrayConverter))]
        public Tile[,] gameBoard {get; set;}
        public (int, int) startPos { get; set; }
        public (int, int) endPos { get; set; }
        #endregion
        
        #region Events
        public event EventHandler<(int x, int y)>? EntityDied;
        public event EventHandler<(int x, int y)>? TileRemoved;
        #endregion

        #region Constructors
        public GameTable()
        {
            animals = new List<Animal>();
            rangers = new List<Ranger>();
            poachers = new List<Poacher>();
            vehicles = new List<Jeep>();
            gameBoard = new Tile[30,60];
            satisfaction = 50;
        }
        #endregion

        #region EventHandlers
        public void OnEntityDied((int x, int y) coords){
            EntityDied?.Invoke(this, coords);
        }
        public void OnTileRemoved((int x, int y) coords){
            TileRemoved?.Invoke(this, coords);
        }
        #endregion

    }
}
