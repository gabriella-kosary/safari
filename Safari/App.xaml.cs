using Microsoft.Win32;
using Safari.Model;
using Safari.Persistence;
using Safari.Persistence.Entities;
using Safari.Persistence.Tiles;
using Safari.ViewModel;
using Safari.Views;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

namespace Safari
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        #region Fields
        private int _canvasHeight;
        private int _canvasWidth;
        private Game _model = new Game();
        private MainViewModel? _mainViewModel = null;
        private MainWindow? _mainWindow = null;
        private MenuViewModel? _menuViewModel = null;
        private TutorialViewModel? _tutorialViewModel = null;
        private GameViewModel? _gameViewModel = null;
        //private MenuView? _menuView = null;
        //private GameView? _gameView = null;
        private DispatcherTimer? _timer = null;
        #endregion

        public App() {
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e) {
            _timer = new DispatcherTimer(DispatcherPriority.Send);
            _timer.Tick += _model.GameProgress;
            _timer.IsEnabled = false;
            _mainViewModel = new MainViewModel(_model);
            _menuViewModel = new MenuViewModel(_model);
            _menuViewModel.StartEvent += ViewModels_ChangePages;
            _menuViewModel.LoadGameEvent += MenuViewModel_LoadGameEvent;
            _menuViewModel.ExitEvent += MenuViewModel_ExitEvent;
            _tutorialViewModel = new TutorialViewModel();
            _tutorialViewModel.BackEvent += ViewModels_ChangePages;
            _gameViewModel = new GameViewModel(_model);
            _gameViewModel.NewGameStarting += GameViewModel_NewGameStarting;
            _gameViewModel.SpeedChaning += GameViewModel_SpeedChanged;
            _gameViewModel.StartEvent += ViewModels_ChangePages;
            _gameViewModel.ExitEvent += ViewModels_ChangePages;
            _gameViewModel.ModelEntityMoved += Model_EntityMoved;
            _gameViewModel.ModelTilesCreated += Model_TilesCreated;
            _gameViewModel.ModelEntityPlaced += Model_EntityPlaced;
            _gameViewModel.ModelCanSeeChanged += Model_CanSeeChanged;
            _gameViewModel.ModelEntityDied += Model_EntityDied;
            _gameViewModel.ModelAnimalStateChanged += Model_AnimalStateChanged;
            _gameViewModel.ModelGameFinished += Model_GameFinished;

            _mainWindow = new MainWindow {
                DataContext = _mainViewModel
            };
            _mainWindow.Show();
            // Request navigation to change to menuView
            ChangeToMenu();
        }

        #region Model Functions
        private void Model_GameFinished(object? sender, string e) {
            _timer?.Stop();
            var answer = MessageBox.Show(e + "\nSzeretnél új játékot kezdeni?", "Game Over", MessageBoxButton.YesNo);
            if(answer == MessageBoxResult.Yes) {
                ViewModels_ChangePages(this, ("Game","Medium", false));
            }
        }
        private void Model_EntityDied(object? sender, PlacedEventargs e) {
            Dispatcher.InvokeAsync(() => {
                if (_gameViewModel == null) throw new NullReferenceException("gameViewModel was null");
                int cnt = 0;
                for (int i = 1800; i < _gameViewModel.Fields.Count; i++) {
                    GameField field = _gameViewModel.Fields[i];
                    if (field.IsAnimal && e.IsAnimal) {
                        cnt++;
                    } else if (field.Type == e.Type) {
                        cnt++;
                    }
                    if (field.X == e.X && field.Y == e.Y && field.Type == e.Type && cnt - 1 == e.Index) {
                        //_gameViewModel.Fields[i].Type = "Empty";
                        if(field.Type == "Ranger" && _gameViewModel.Fields[i] == _gameViewModel.ControlledRanger) {
                            _gameViewModel.ControlledRanger = null;
                            _gameViewModel.IsRangerSelected = false;
                            _gameViewModel.IsPoacherSelected = false;
                            _gameViewModel.IsCarnivoreSelected = false;
                            _gameViewModel.IsHerbivoreSelected = false;
                        }
                        if(_gameViewModel.IsPoacherSelected && field.Type == "Poacher" && _gameViewModel.Fields[i] == _gameViewModel.EntityToBeKilledByRanger) {
                            _gameViewModel.EntityToBeKilledByRanger = null;
                            _gameViewModel.IsPoacherSelected = false;
                        }
                        if(_gameViewModel.IsCarnivoreSelected  && field.IsCarnivore && _gameViewModel.Fields[i] == _gameViewModel.EntityToBeKilledByRanger) {
                            _gameViewModel.EntityToBeKilledByRanger = null;
                            _gameViewModel.IsCarnivoreSelected = false;
                        }
                        if(_gameViewModel.IsHerbivoreSelected  && field.IsHerbivore && _gameViewModel.Fields[i] == _gameViewModel.EntityToBeKilledByRanger) {
                            _gameViewModel.EntityToBeKilledByRanger = null;
                            _gameViewModel.IsHerbivoreSelected = false;
                        }
                        _gameViewModel.Fields.RemoveAt(i);
                        return;
                    }
                }
            }, DispatcherPriority.Render);
        }
        private void Model_EntityPlaced(object? sender, PlacedEventargs e) {
            Dispatcher.InvokeAsync(() => {
                _gameViewModel?.Fields.Add(new GameField {
                    Type = e.Type,
                    Image = $"{e.Type}_Base",
                    X = e.X,
                    Y = e.Y,
                    XY = new Tuple<int, int>(e.X, e.Y),
                    Width = (_canvasWidth / 60),
                    Height = (_canvasHeight / 30),
                    IsEntity = true,
                    IsAnimal = e.IsAnimal,
                    CanSee = !e.Type.Equals("Poacher"),
                    IsHerbivore = e.IsHerbivore,
                    IsCarnivore = e.IsCarnivore
                });
            }, DispatcherPriority.Render);
        }
        private void Model_EntityMoved(object? sender, EntityMovedEventArgs e) {

            Dispatcher.InvokeAsync(() => {
                if (_gameViewModel == null) throw new NullReferenceException("gameViewModel was null");
                int cnt = 0;
                for (int i = 1800; i < _gameViewModel.Fields.Count; i++) {
                    GameField field = _gameViewModel.Fields[i];
                    if (field.IsAnimal && e.IsAnimal) {
                        cnt++;
                    } else if (field.Type == e.Type) {
                        cnt++;
                    }
                    if (field.X == e.OldX && field.Y == e.OldY && field.Type == e.Type && cnt - 1 == e.Index) {
                        field.X = e.NewX;
                        field.Y = e.NewY;
                        field.XY = new Tuple<int, int>(e.NewX, e.NewY);
                        field.Destination = e.Destination;
                    }
                }
            }, DispatcherPriority.Render);
        }
        private void Model_AnimalStateChanged(object? sender, AnimalStateEventargs e)
        {
            Dispatcher.InvokeAsync(() => {
                if (_gameViewModel == null) throw new NullReferenceException("gameViewModel was null");
                int cnt = 0;
                for (int i = 1800; i < _gameViewModel.Fields.Count; i++)
                {
                    GameField field = _gameViewModel.Fields[i];
                    if (field.IsAnimal)
                    {
                        cnt++;
                    }
                    else if (field.Type == e.Type)
                    {
                        cnt++;
                    }
                    if (field.X == e.X && field.Y == e.Y && field.Type == e.Type && cnt - 1 == e.Index)
                    {
                        field.Image = $"{e.Type}_{e.State}";
                    }
                }
            }, DispatcherPriority.Render);
        }
        private void Model_CanSeeChanged(object? sender, PlacedEventargs e) {
            if (_gameViewModel == null) throw new NullReferenceException("gameViewModel was null");
            Dispatcher.InvokeAsync(() => {
                int cnt = 0;
                for (int i = 1800; i < _gameViewModel.Fields.Count; i++) {
                    GameField field = _gameViewModel.Fields[i];
                    if (field.IsAnimal && e.IsAnimal) {
                        cnt++;
                    } else if (field.Type == e.Type) {
                        cnt++;
                    }
                    if (field.X == e.X && field.Y == e.Y && field.Type == e.Type && cnt - 1 == e.Index) {
                       if (field.CanSee != e.Cansee) {
                            field.CanSee = e.Cansee;
                       }
                        return;
                    }
                }
            }, DispatcherPriority.Render);
        }
        private void Model_TilesCreated(object? sender, TilesCreatedEventArgs eventArgs) {
            Dispatcher.InvokeAsync(async () => {
                _canvasHeight = eventArgs.CanvasHeight;
                _canvasWidth = eventArgs.CanvasWidth;
                foreach (var tile in eventArgs.Tiles) {
                    String actutalType = tile.position == eventArgs.EndPosition || tile.position == eventArgs.StartPosition ? "StartEnd" : tile.GetType().Name;
                    await Dispatcher.InvokeAsync(() => {
                        _gameViewModel?.Fields.Add(new GameField {
                            X = tile.position.Item1,
                            Y = tile.position.Item2,
                            XY = new Tuple<int, int>(tile.position.Item1, tile.position.Item2),
                            IsAnimal = false,
                            Type = actutalType,
                            Width = (eventArgs.CanvasWidth / 60),
                            Height = (eventArgs.CanvasHeight / 30),
                            CanSee = true
                        });
                    }, DispatcherPriority.Render);
                }
                foreach (var entity in eventArgs.Enitites) {
                    await Dispatcher.InvokeAsync(() => {
                        _gameViewModel?.Fields.Add(new GameField {
                            X = entity.position.Item1,
                            Y = entity.position.Item2,
                            XY = new Tuple<int, int>(entity.position.Item1, entity.position.Item2),
                            IsEntity = true,
                            IsAnimal = true,
                            Type = entity.GetType().Name,
                            Image = $"{entity.GetType().Name}_Base",
                            Width = (eventArgs.CanvasWidth / 60),
                            Height = (eventArgs.CanvasHeight / 30),
                            CanSee = entity.canSee,
                            IsHerbivore = entity is Herbivore,
                            IsCarnivore = entity is Carnivore
                        });
                    }, DispatcherPriority.Render);
                }
                //waiting for the map to generate
                await Task.Delay(5000);
                if (_timer == null) throw new NullReferenceException("timer was null");
                if (_gameViewModel == null) throw new NullReferenceException("gameViewModel was null");
                _timer.Interval = TimeSpan.FromMilliseconds(_model.SpeedChanged(1));
                _timer?.Start();
            }, DispatcherPriority.Render);
        }
        #endregion

        #region GameViewModel Functions
        private void GameViewModel_NewGameStarting(object? sender, EventArgs e) {
            _timer?.Stop();
        }

        private void GameViewModel_SpeedChanged(object? sender, string e) {
            if (_timer == null) throw new Exception("Timer was null");
            _timer.Interval = TimeSpan.FromMilliseconds(_model.SpeedChanged(int.Parse(e)));
        }
        #endregion

        #region MenuViewModel Functions
        private void MenuViewModel_LoadGameEvent(object? sender, EventArgs e) {
            _gameViewModel?.Reset();
            _timer?.Stop();
            try {
                _model.LoadGame();
                PageNavigation.RequestNavigation(new GameView {
                    DataContext = _gameViewModel
                });
            } catch (Exception exception) {
                MessageBox.Show($"Something went wrong while loading the saved game. {exception.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Private Functions
        private void ChangeToMenu() {
            _menuViewModel?.Reset();
            _timer?.Stop();
            PageNavigation.RequestNavigation(new MenuView {
                DataContext = _menuViewModel
            });
        }
        private void ChangeToTutorial(String level) {
            _timer?.Stop();
            if (_tutorialViewModel == null) throw new Exception("TutorialViewModel was null");
            _tutorialViewModel.Level = level;
            PageNavigation.RequestNavigation(new TutorialView {
                DataContext = _tutorialViewModel
            });
        }
        private void ChangeToGame(String level) {
            _gameViewModel?.Reset();
            _timer?.Stop();
            PageNavigation.RequestNavigation(new GameView {
                DataContext = _gameViewModel
            });

            _model.NewGame(ConvertLevel(level));
        }

        private int ConvertLevel(string level) {
            switch (level) {
                case "Easy":
                    return 0;
                case "Medium":
                    return 1;
                case "Hard":
                    return 2;
                default:
                    throw new ArgumentException($"Wrong level given to the converter: {level}");
            }
        }
        #endregion

        #region EventHandler Functions
        private void ViewModels_ChangePages(object? sender, (string page, string level, bool needsConfirmation) e) {
            switch (e.page) {
                case "Menu":
                    if(e.needsConfirmation) {
                        var answer = MessageBox.Show("Are you sure you want to exit to the menu?", "Choose", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if(answer == MessageBoxResult.Yes) {
                            _timer?.Stop();
                            _model.SaveGame();
                            ChangeToMenu();
                        }
                    } else {
                        ChangeToMenu();
                    }
                    break;
                case "Game":
                    if (e.needsConfirmation) {
                        var answer = MessageBox.Show("Are you sure you want to start a new game?", "Choose", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (answer == MessageBoxResult.Yes) {
                            _timer?.Stop();
                            _model.SaveGame();
                            ChangeToGame(e.level);
                        }
                    } else {
                        ChangeToGame(e.level);
                    }
                    break;
                case "Tutorial":
                    ChangeToTutorial(e.level);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region MenuViewModel Events
        private void MenuViewModel_ExitEvent(object? sender, EventArgs e) {
            var answer = MessageBox.Show("Are you sure you want to exit?", "Choose", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (answer == MessageBoxResult.Yes) {
                Shutdown();
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }

}
