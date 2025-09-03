using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence {
    public class EntityMovedEventArgs : EventArgs {
        #region Fields
        private int oldX;
        private int oldY;
        private int newX;
        private int newY;
        private string? type;
        private (int, int) destination;
        private int _index;
        private bool _isAnimal;
        #endregion

        #region Properties
        public bool IsAnimal {
            get => _isAnimal;
            set {
                _isAnimal = value;
            }
        }
        public int Index {
            get => _index;
            set {
                _index = value;
            }
        }
        public int OldX {
            get => oldX;
            set {
                oldX = value;
            }
        }
        public int OldY {
            get => oldY;
            set {
                oldY = value;
            }
        }
        public int NewX {
            get => newX;
            set {
                newX = value;
            }
        }
        public int NewY {
            get => newY;
            set {
                newY = value;
            }
        }
        public string Type {
            get => type ?? "";
            set {
                type = value;
            }
        }

        public (int, int) Destination
        {
            get { return destination; }
            set
            {
                destination = value;
            }
        }
        #endregion

    }
}
