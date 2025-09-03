using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public abstract class Plant : Tile
    {
        public int maxHp { get; set; }
        public int hp {get; set;}
        public int growTime {get; set;}

        public Plant(bool canSee, (int, int) position) : base(canSee, position) { }
    }
}
