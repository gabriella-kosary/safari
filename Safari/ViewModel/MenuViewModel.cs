using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Safari.Model;
using Safari.Persistence.Entities;
using Safari.Persistence.Tiles;
using Safari.Views;

namespace Safari.ViewModel
{
    public class MenuViewModel : ViewModelBase
    {
        #region Fields
        private String _level = "";
        private Game _model;
        #endregion
        #region Commands
        public DelegateCommand? StartCommand { get; set; }
        public DelegateCommand? TutorialCommand { get; set; }
        public DelegateCommand? LevelCommand { get; set; }
        public DelegateCommand? LoadCommand { get; set; }
        public DelegateCommand? ExitCommand { get; set; }
        #endregion

        #region Properties
        public String Level {
            get => _level;
            set {
                _level = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Events
        public event EventHandler<(String, String, bool)>? StartEvent;
        public event EventHandler? ExitEvent;
        public event EventHandler? LoadGameEvent;
        #endregion

        public MenuViewModel(Game model) {
            _model = model;
            StartCommand = new DelegateCommand(_ => {
                OnStartEvent("Game");
            });
            LevelCommand = new DelegateCommand(level => {
                if (level is String lvl) {
                    Level = lvl;
                }
            });
            ExitCommand = new DelegateCommand(_ => {
                OnExitEvent();
            });

            TutorialCommand = new DelegateCommand(_ =>
            {
                OnStartEvent("Tutorial");
            });
            LoadCommand = new DelegateCommand(_ => {
                OnLoadGameEvent();
            });
        }
        #region Eventhandler functions
        private void OnExitEvent() {
            ExitEvent?.Invoke(this, EventArgs.Empty);
        }
        private void OnLoadGameEvent() {
            LoadGameEvent?.Invoke(this, EventArgs.Empty);
        }
        private void OnStartEvent(String page) {
            StartEvent?.Invoke(this, (page, Level, false));
        }
        #endregion

        #region private functions
        public void Reset() {
            Level = "Medium";
        }
        #endregion
    }
}