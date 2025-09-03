using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public class Tree : Plant
    {

        public Tree((int, int) position) : base(true, position)
        {
            maxHp = 300;
            hp = 0;
            growTime = 15;
        }
    }
}
