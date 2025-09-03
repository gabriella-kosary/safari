using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Tiles
{
    public class Lake : Water
    {
        public Lake((int, int) position) : base(true, position) { }
    }
}