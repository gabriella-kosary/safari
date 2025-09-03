using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public class Bush : Plant
    {

        public Bush((int, int) position) : base(true, position)
        {
            maxHp = 150;
            hp = 0;
            growTime = 10;
        }
    }
}
