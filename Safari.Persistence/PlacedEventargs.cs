using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence
{
    public class PlacedEventargs : EventArgs
    {
        private int x;
        private int y;
        private int index;
        private string type = "";
        private bool _isAnimal;
        private bool _canSee;
        private bool _isCarnivore;
        private bool _isHerbivore;

        public bool IsHerbivore {
            get { return _isHerbivore; }
            set { _isHerbivore = value; }
        }


        public bool IsCarnivore {
            get { return _isCarnivore; }
            set { _isCarnivore = value; }
        }


        public bool Cansee {
            get { return _canSee; }
            set { _canSee = value; }
        }


        public bool IsAnimal {
            get { return _isAnimal; }
            set { _isAnimal = value; }
        }


        public int X {
            get => x;
            set {
                x = value;
            }
        }
        public int Y {
            get => y;
            set {
                y = value;
            }
        }

        public int Index {
            get => index;
            set {
                index = value;
            }
        }
        public string Type {
            get => type ?? "";
            set {
                type = value;
            }
        }
    }
}
