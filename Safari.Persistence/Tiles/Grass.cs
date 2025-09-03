using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public class Grass : Plant
    {

        public Grass((int, int) position) : base(true, position)
        {
            maxHp = 80;
            hp = 0;
            growTime = 5;
        }
    }
}
