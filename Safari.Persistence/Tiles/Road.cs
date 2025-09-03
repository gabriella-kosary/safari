using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public class Road : Tile
    {
        public Road((int, int) position) : base(true, position) { }
    }
}
