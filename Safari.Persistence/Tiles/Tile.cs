using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public abstract class Tile
    {
        public bool canSee {  get; set; }
        public (int, int) position { get; set; }

        public Tile(bool canSee, (int, int) position)
        {
            this.canSee = canSee;
            this.position = position;
        }
    }
}
