using Safari.Persistence.Entities;
using Safari.Persistence.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Safari.Model;
using Safari.Persistence;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Input;

namespace Safari.ViewModel
{
    public class GameViewModel : ViewModelBase {
        #region Fields
        private readonly String[] listItems = {
            "Grass",
            "Bush",
            "Tree",
            "Lake",
            "Elephant",
            "Antilope",
            "Lion",
            "Hyena",
            "Road",
            "Jeep",
            "Ranger",
            "Chip"
        };
        private Game _model;
        public ObservableCollection<GameField> Fields { get; set; }

        private int _money;
        private int _carnivores;
        private int _herbivores;
        private int _days;
        private int _tourists;
        private int _selectedIndex;
        private String? _buyColor;
        private String? _killColor;
        private bool isKillSelected => KillColor == "Red";
        private bool _isRangerSelected;
        private GameField? _controlledRanger;
        private GameField? _entityToBeKilledByRanger;
        private bool _isHerbivoreSelected;
        private bool _isCarnivoreSelected;
        private bool _isPoacherSelected;
        private bool _isThereIncompleteRoad;
        private String? _debugDestination;
        private bool _isNight = true;
        #endregion

        #region Properties
        public GameField? ControlledRanger {
            get => _controlledRanger;
            set {
                _controlledRanger = value;
                OnPropertyChanged();
            }
        }
        public GameField? EntityToBeKilledByRanger {
            get => _entityToBeKilledByRanger;
            set {
                _entityToBeKilledByRanger = value;
                OnPropertyChanged();
            }
        }
        public bool IsThereIncompleteRoad {
            get { return _isThereIncompleteRoad; }
            set { 
                if(IsThereIncompleteRoad != value) {
                    _isThereIncompleteRoad = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsHerbivoreSelected {
            get { return _isHerbivoreSelected; }
            set { 
                _isHerbivoreSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsPoacherSelected {
            get { return _isPoacherSelected; }
            set { 
                _isPoacherSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsCarnivoreSelected {
            get { return _isCarnivoreSelected; }
            set { 
                _isCarnivoreSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsBuySelected => BuyColor == "Gray";

        public bool IsRangerSelected {
            get => _isRangerSelected;
            set {
                _isRangerSelected = value;
                OnPropertyChanged();
            }
        }
        public String DebugDestination
        {
            get => _debugDestination ?? "";
            set
            {
                _debugDestination = value;
                OnPropertyChanged();
            }
        }
        public int SelectedIndex {
            get { return _selectedIndex; }
            set {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }
        public int Money { 
            get => _money; 
            set {
                _money = value;
                OnPropertyChanged();
            }
        }
        public String BuyColor {
            get => _buyColor ?? "";
            set {
                _buyColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBuySelected));
            }
        }

        public int Carnivores { 
            get => _carnivores;
            set {
                _carnivores = value;
                OnPropertyChanged();
            } 
        }
        public int Herbivores {
            get => _herbivores;
            set {
                _herbivores = value;
                OnPropertyChanged();
            }
        }
        public int Tourists {
            get => _tourists;
            set {
                _tourists = value;
                OnPropertyChanged();
            }
        }
        public int Days {
            get => _days;
            set {
                _days = value;
                OnPropertyChanged();
            }
        }

        public String KillColor {
            get => _killColor ?? "";
            set {
                _killColor = value;
                OnPropertyChanged();
            }
        }
        public bool IsNight
        {
            get => _isNight;
            set
            {
                _isNight = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Events
        public event EventHandler<(String, String, bool)>? StartEvent;
        public event EventHandler<(String, String, bool)>? ExitEvent;
        public event EventHandler<EntityMovedEventArgs>? ModelEntityMoved;
        public event EventHandler<TilesCreatedEventArgs>? ModelTilesCreated;
        public event EventHandler<PlacedEventargs>? ModelEntityPlaced;
        public event EventHandler<PlacedEventargs>? ModelCanSeeChanged;
        public event EventHandler<PlacedEventargs>? ModelEntityDied;
        public event EventHandler<AnimalStateEventargs>? ModelAnimalStateChanged;
        public event EventHandler<String>? SpeedChaning;
        public event EventHandler? NewGameStarting;
        public event EventHandler<String>? ModelGameFinished;
        #endregion

        #region Commands
        public DelegateCommand? StartCommand { get; set; }
        public DelegateCommand? BuyCommand { get; set; }
        public DelegateCommand? KillCommand { get; set; }
        public DelegateCommand? NewGameCommand { get; set; }

        public DelegateCommand ExitCommand { get; set; }
        //intként fogjuk átadni a speedet
        public DelegateCommand SpeedCommand { get; set; }
        public DelegateCommand RectangleClickCommand { get; set; }
        public DelegateCommand RectangleHoverCommand { get; set; }
        public DelegateCommand BuyColorCommand { get; set; }
        public DelegateCommand KillColorCommand { get; set; }
        #endregion

        public GameViewModel(Game? model)
        {
            if(model == null) { throw new ArgumentException("The model is null"); }
            _model = model;
            Fields = new ObservableCollection<GameField>();
            _model.MoneyChanged += new EventHandler<int>(Model_MoneyChanged);
            _model.EntityMoved += new EventHandler<EntityMovedEventArgs>(Model_EntityMoved);
            _model.EntityPlaced += new EventHandler<PlacedEventargs>(Model_EntityPlaced);
            _model.EntityDied += new EventHandler<PlacedEventargs>(Model_EntityDied);
            _model.AnimalStateChanged += new EventHandler<AnimalStateEventargs>(Model_AnimalStateChanged);
            _model.TilePlaced += new EventHandler<PlacedEventargs>(Model_TilePlaced);
            _model.GameFinished += new EventHandler<String>(Model_GameFinished);
            _model.IncompleteRoad += new EventHandler<bool>(Model_IncompliteRoad);
            _model.TilesCreated += new EventHandler<TilesCreatedEventArgs>(Model_TileCreated);
            _model.DaysChanged += new EventHandler<int>(Model_DaysChanged);
            _model.CanSeeChanged += new EventHandler<PlacedEventargs>(Model_CanSeeChanged);
            _model.DayNightChanged += new EventHandler<bool>(Model_DayNightsChanged);
            _model.StatsChanged += new EventHandler<StatEventArgs>(Model_StatsChanged);
            
            StartCommand = new DelegateCommand(param => View_GameStart());
            ExitCommand = new DelegateCommand(param => View_GameExit());
            RectangleClickCommand = new DelegateCommand(param => View_RectangleClick(param));
            RectangleHoverCommand = new DelegateCommand(param => View_RectangleHover(param));
            BuyColorCommand = new DelegateCommand(param => View_BuyColorCommand());
            KillColorCommand = new DelegateCommand(param => View_KillColorCommand());
            // na hátha mostmár sikerül ezt is hozzáadni.....
            SpeedCommand = new DelegateCommand(param => View_SpeedCommand(param));

            Money = 0;
            Carnivores = 0;
            Herbivores = 0;
            Tourists = 0;
            Days = 0;
            SelectedIndex = 0;
            BuyColor = "Bisque";
            KillColor = "Bisque";
        }

        #region View_Methods
        private void View_SpeedCommand(object? param) {
            if(param != null && param is String speed) {
                OnSpeedChaning(speed);
            }
        }
        private void View_KillColorCommand() {
            if (KillColor == "Red") {
                KillColor = "Bisque";
            } else {
                KillColor = "Red";
                BuyColor = "Bisque";
            }
        }
        private void View_BuyColorCommand() {
            if (BuyColor == "Gray") {
                BuyColor = "Bisque";
            } else {
                BuyColor = "Gray";
                KillColor = "Bisque";
            }
        }
        private void View_GameStart()
        {
            OnStartEvent("Game", "Medium", true);
        }
        private void View_GameExit()
        {
            OnExitEvent("Menu", "", true);
        }
        private void View_RectangleClick(object? param) {
            if (param is GameField gameField) {
                if (IsBuySelected) {
                    if (listItems[SelectedIndex] == "Chip" && !gameField.IsAnimal) return;
                    _model.Buy(listItems[SelectedIndex], (gameField.XY.Item1, gameField.XY.Item2));
                } else if (isKillSelected) {
                    if(IsRangerSelected) {
                        // Maybe it would be better to have a cnt instead of index
                        // because the cnt's precision is better
                        if (ControlledRanger == null) throw new Exception("ControlledRanger was null");
                        if(IsRangerSelected && (!Fields.Contains(ControlledRanger) || ControlledRanger.Type != "Ranger")) IsRangerSelected = false;
                        if(gameField.IsEntity && gameField.Type != "Ranger") {
                            _model.KillWithRanger(ControlledRanger.XY.ToValueTuple(),
                                gameField.XY.ToValueTuple());
                            IsRangerSelected = false;
                            if(gameField.IsCarnivore) {
                                IsCarnivoreSelected = true;
                            } else if(gameField.IsHerbivore) {
                                IsHerbivoreSelected = true;
                            } else if(gameField.Type == "Poacher") {
                                IsPoacherSelected = true;
                            }
                            EntityToBeKilledByRanger = gameField;
                        }
                    } else {
                        if(gameField.Type == "Ranger") {
                            for (int i = 1800; i < Fields.Count; i++) {
                                if (Fields[i] == gameField) {
                                    ControlledRanger = Fields[i];
                                    EntityToBeKilledByRanger = null;
                                    IsRangerSelected = true;
                                    IsCarnivoreSelected = false;
                                    IsHerbivoreSelected = false;
                                    IsPoacherSelected = false;
                                }
                            }
                        }
                    }
                } else {
                    DebugDestination = $"({gameField.X}, {gameField.Y})";
                }
            }
        }
        private void View_RectangleHover(object? param) {
            if(Mouse.LeftButton == MouseButtonState.Pressed) {
                if(param is GameField gameField && gameField.Type != "Road" && listItems[SelectedIndex] == "Road") {
                    _model.Buy(listItems[SelectedIndex], (gameField.XY.Item1, gameField.XY.Item2));
                }
            }
        }
        #endregion

        #region Model_Methods
        private void Model_StatsChanged(object? sender, StatEventArgs e) {
            Tourists = e.Visitors;
            Carnivores = e.Carnivores;
            Herbivores = e.Herbivores;
        }
        private void Model_DayNightsChanged(object? sender, bool e) {
            IsNight = !e;
        }
        private void Model_CanSeeChanged(object? sender, PlacedEventargs e) {
            OnModelCanSeeChanged(e);
        }
        private void Model_DaysChanged(object? sender, int e) {
            Days = e;
        }
        private void Model_TileCreated(object? sender, TilesCreatedEventArgs eventArgs) {
            OnModelTileCreated(eventArgs);
        }

        private void Model_IncompliteRoad(object? sender, bool e)
        {
            IsThereIncompleteRoad = e;
        }
        private void Model_GameFinished(object? sender, string e)
        {
            OnModelGameFinished(e);
        }
        private void Model_TilePlaced(object? sender, PlacedEventargs e)
        {
            foreach (var field in Fields) {
                if (field.X == e.X && field.Y == e.Y) {
                    field.Type = e.Type;
                }
            }
        }
        private void Model_EntityDied(object? sender, PlacedEventargs e)
        {
            OnModelEntityDied(e);
        }
        private void Model_EntityPlaced(object? sender, PlacedEventargs e) {
            OnModelEntityPlaced(e);
        }

        private void Model_EntityMoved(object? sender, EntityMovedEventArgs e)
        {
            OnModelEntityMoved(e);
        }
        private void Model_AnimalStateChanged(object? sender, AnimalStateEventargs e)
        {
            OnAnimalStateChanged(e);
        }

        private void Model_MoneyChanged(object? sender, int e)
        {
            Money = e;
        }
        #endregion

        #region EventHandler Functions
        private void OnModelEntityMoved(EntityMovedEventArgs e) {
            ModelEntityMoved?.Invoke(this, e);
        }
        private void OnModelCanSeeChanged(PlacedEventargs e) {
            ModelCanSeeChanged?.Invoke(this, e);
        }
        private void OnStartEvent(string page, string level, bool needsConfiation) {
            StartEvent?.Invoke(this, (page, level, needsConfiation));
            IsNight = false;
        }
        private void OnExitEvent(string page, string level, bool needsConfiation) {
            ExitEvent?.Invoke(this, (page, level, needsConfiation));
        }

        private void OnModelTileCreated(TilesCreatedEventArgs eventArgs) {
            ModelTilesCreated?.Invoke(this, eventArgs);
        }
        private void OnModelEntityPlaced(PlacedEventargs e) {
            ModelEntityPlaced?.Invoke(this, e);
        }
        private void OnModelEntityDied(PlacedEventargs e) {
            ModelEntityDied?.Invoke(this, e);
        }
        private void OnAnimalStateChanged(AnimalStateEventargs e)
        {
            ModelAnimalStateChanged?.Invoke(this, e);
        }
        private void OnSpeedChaning(string speed) {
            SpeedChaning?.Invoke(this, speed);
        }
        private void OnNewGameStarting() {
            NewGameStarting?.Invoke(this, EventArgs.Empty);
        }
        private void OnModelGameFinished(string e) {
            ModelGameFinished?.Invoke(this, e);
        }
        #endregion


        public void Reset() {
            Fields.Clear();
            Money = 0;
            Carnivores = 0;
            Herbivores = 0;
            Tourists = 0;
            Days = 0;
            SelectedIndex = 0;
            BuyColor = "Bisque";
            KillColor = "Bisque";

            IsRangerSelected = false;
            IsCarnivoreSelected = false;
            IsHerbivoreSelected = false;
            IsPoacherSelected = false;
            IsThereIncompleteRoad = false;
        }
    }
}
