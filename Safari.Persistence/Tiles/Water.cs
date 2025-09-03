using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public abstract class Water : Tile
    {
        public Water(bool canSee, (int, int) position) : base(canSee, position) { }

    }
}