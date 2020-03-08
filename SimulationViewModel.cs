using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Courier
{
    public class SimulationViewModel : INotifyPropertyChanged
    {
        private Simulation _simulation = null;
        private Task _currentSimTask = null;
        private CancellationTokenSource _stopSimSource = null;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public ObservableCollection<UIElement> WorldObjects { get; set; }

        public RelayCommand<object> NewSimCommand => this._newSimCommand;
        private RelayCommand<object> _newSimCommand;

        public RelayCommand<object> ContinueSimCommand => this._continueSimCommand;
        private RelayCommand<object> _continueSimCommand;

        public RelayCommand<object> StopSimCommand => this._stopSimCommand;
        private RelayCommand<object> _stopSimCommand;

        

        /*public GameSession Session
        {
            get { return this._Session; }
            set
            {
                if (this._Session != value)
                {
                    this._Session = value;
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Session)));
                }
            }
        }*/

        public SimulationViewModel()
        {
            WorldObjects = new ObservableCollection<UIElement>();

            _newSimCommand = new RelayCommand<object>(p =>
            {
                if (_newSimCommand.CanExecute(null))
                {
                    var generator = new WorldGenerator();
                    var map = WorldGenerator.GetDefaultMap();
                    World world = generator.Generate(map);

                    WorldObjects.Clear();
                    foreach (var obj in world.Objects)
                        WorldObjects.Add(obj.View);

                    if (generator.Robot != null)
                    {
                        System.Windows.Window window = new ProbabilityWindow(generator.Robot.NavigationSystem);
                        window.Show();
                    }
                    _simulation = new Simulation(world);

                    _stopSimSource = new CancellationTokenSource();
                    _currentSimTask = GetSimulationWrapperTask();
                    CommandManager.InvalidateRequerySuggested();
                }
            }, p =>
            {
                return _currentSimTask == null;
            });

            _continueSimCommand = new RelayCommand<object>(obj =>
            {
                if (_continueSimCommand.CanExecute(null))
                {
                    _stopSimSource = new CancellationTokenSource();
                    _currentSimTask = GetSimulationWrapperTask();
                    CommandManager.InvalidateRequerySuggested();
                }
            }, p =>
            {
                return _currentSimTask == null && _simulation != null;
            });

            _stopSimCommand = new RelayCommand<object>(obj =>
            {
                var source = _stopSimSource;
                object locker = new object();
                lock (locker)
                {
                    if (source != null && !source.IsCancellationRequested)
                        source.Cancel(false);
                }
            }, p =>
            {
                return this._stopSimSource != null;
            });
        }

        private async Task GetSimulationWrapperTask()
        {
            await _simulation.SimulateAsync(_stopSimSource.Token);
            _stopSimSource.Dispose();
            _stopSimSource = null;
            _currentSimTask = null;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}