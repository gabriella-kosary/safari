using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence
{
    public class AnimalStateEventargs : EventArgs
    {
        private int x;
        private int y;
        private int index;
        private string type = "";
        private string state = ""; 
        public int X
        {
            get => x;
            set
            {
                x = value;
            }
        }
        public int Y
        {
            get => y;
            set
            {
                y = value;
            }
        }

        public int Index
        {
            get => index;
            set
            {
                index = value;
            }
        }
        public string Type
        {
            get => type ?? "";
            set
            {
                type = value;
            }
        }
        public string State
        {
            get => state ?? "";
            set
            {
                state = value;
            }
        }
    }
}
