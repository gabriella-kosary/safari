using System.Xml;

namespace Safari.ViewModel
{
    public class GameField :ViewModelBase
    {
        private int x;
        private int y;
        // Ha képet használunk
        private String? _image;
        private String? type;
        private int _width;
        private int _height;
        private (int, int) _destination;
        private bool _isEntity;
        private bool _isAnimal;
        private bool _canSee;
        private bool _isCarnivore;
        private bool _isHerbivore;

        public bool IsHerbivore {
            get { return _isHerbivore; }
            set { 
                _isHerbivore = value;
                OnPropertyChanged();
            }
        }


        public bool IsCarnivore {
            get { return _isCarnivore; }
            set { 
                _isCarnivore = value;
                OnPropertyChanged();
            }
        }


        public bool CanSee {
            get { return _canSee; }
            set { 
                _canSee = value;
                OnPropertyChanged();
            }
        }



        public bool IsAnimal {
            get { 
                return _isAnimal;
            }
            set { 
                _isAnimal = value;
                OnPropertyChanged();
            }
        }


        public bool IsEntity {
            get { return _isEntity; }
            set {
                _isEntity = value;
                OnPropertyChanged();
            }
        }

        public int X {
            get { return x; }
            set {
                x = value;
                OnPropertyChanged();
            }
        }
        public int Y {
            get { return y; }
            set { 
                y = value;
                OnPropertyChanged();
            }
        }

        

        public int Height {
            get { return _height; }
            set {
                _height = value;
                OnPropertyChanged();
            }
        }


        public int Width {
            get { return _width; }
            set { 
                _width = value;
                OnPropertyChanged();
            }
        }


        public String Type {
            get { return type ?? ""; }
            set { 
                type = value;
                OnPropertyChanged();
            }
        }

        public String Image
        {
            get { return _image ?? ""; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }
        // Ha színt használnánk
        private String? _color;
        public String Color
        {
            get { return _color ?? ""; }
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }
        private Tuple<int, int>? xy;

        public Tuple<int, int> XY
        {
            get { return xy ?? (0,0).ToTuple(); }
            set {
                xy = value;
                OnPropertyChanged();
            }
        }

        public (int, int) Destination
        {
            get => _destination;
            set
            {
                _destination = value;
                OnPropertyChanged();
            }
        }
    }
}