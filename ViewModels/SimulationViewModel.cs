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
    public class SimulationViewModel
    {
        private Simulation _simulation = null;
        private volatile bool _isSimulating = false;
        private CancellationTokenSource _stopSimSource = null;
        private int _floor = 0;
        private NavigationSystem _navigationSystem = null;
        private readonly ObservableCollection<WorldObject> _worldObjects;
        private LocalizationWindow _localizationWindow = null;

        public int[] WorldShape => _navigationSystem?.WorldShape;
        public double CanvasSize  { get; set; }

        public ObservableCollection<UIElement> FloorObjects { get; }
        public ObservableCollection<UIElement> LocalizationIndicators { get; }

        public RelayCommand<object> NewSimCommand { get; }
        public RelayCommand<object> ContinueSimCommand { get; }
        public RelayCommand<object> StepSimCommand { get; }
        public RelayCommand<object> StopSimCommand { get; }
        public RelayCommand<object> ChangeFloorCommand { get; }
        public RelayCommand<object> OpenLocalizationCommand { get; }

        public SimulationViewModel()
        {
            _worldObjects = new ObservableCollection<WorldObject>();
            
            FloorObjects = new ObservableCollection<UIElement>();
            LocalizationIndicators = new ObservableCollection<UIElement>();

            NewSimCommand = new RelayCommand<object>(p =>
            {
                _worldObjects.CollectionChanged -= WorldObjects_CollectionChanged;
                _worldObjects.Clear();

                var generator = new WorldGenerator(CanvasSize);
                string[,,] map = WorldGenerator.ParseMapFromJson("Map.json");
                generator.Settings = ProbabilitySettings.ParseFromJson("Settings.json");
                World world = generator.Generate(map, _worldObjects);
                _worldObjects.CollectionChanged += WorldObjects_CollectionChanged;

                if (generator.Robot != null)
                {
                    if (_navigationSystem != null)
                        _navigationSystem.LocalizationUpdated -= NavigationSystem_LocalizationUpdated;

                    _navigationSystem = generator.Robot.NavigationSystem;
                    _navigationSystem.LocalizationUpdated += NavigationSystem_LocalizationUpdated;
                    
                    _localizationWindow?.Close();
                }
                _simulation = new Simulation(world);
                _floor = 0;

                UpdateFloorObjects(_floor);
                StartSimulationWrapper();
                CommandManager.InvalidateRequerySuggested();
            }, p =>
            {
                return !_isSimulating;
            });

            ContinueSimCommand = new RelayCommand<object>(p =>
            {
                StartSimulationWrapper();
                CommandManager.InvalidateRequerySuggested();
            }, p =>
            {
                return !_isSimulating && _simulation != null;
            });

            StepSimCommand = new RelayCommand<object>(p =>
            {
                StartSimulationStepWrapper();
                CommandManager.InvalidateRequerySuggested();
            }, p =>
            {
                return !_isSimulating && _simulation != null;
            });

            StopSimCommand = new RelayCommand<object>(p =>
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
                return _stopSimSource != null;
            });

            ChangeFloorCommand = new RelayCommand<object>(p =>
            {
                _floor += (int)p;
                UpdateFloorObjects(_floor);
                UpdateLocalizationIndicators(_floor);
                CommandManager.InvalidateRequerySuggested();
            }, p =>
            {
                var world = _simulation?.World;
                if (world == null)
                    return false;
                int destination = (int)p + _floor;
                return destination >= 0 && destination <= world.UpperBound.Z;
            });

            OpenLocalizationCommand = new RelayCommand<object>(p =>
            {
                LocalizationWindow window = _localizationWindow;
                if (window == null)
                {
                    _localizationWindow = window = new LocalizationWindow(this);
                    window.Closed += LocalizationWindow_Closed;
                    window.Show();
                    UpdateLocalizationIndicators(_floor);
                }
                else
                {
                    window.Focus();
                }
                
                
            }, p =>
            {
                return _simulation != null;
            });
        }

        private async void StartSimulationWrapper()
        {
            _stopSimSource = new CancellationTokenSource();
            _isSimulating = true;
            await _simulation.SimulateAsync(_stopSimSource.Token);
            _stopSimSource.Dispose();
            _stopSimSource = null;
            _isSimulating = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private async void StartSimulationStepWrapper()
        {
            _isSimulating = true;
            _simulation.Step();
            _isSimulating = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateFloorObjects(int floor)
        {
            FloorObjects.Clear();
            foreach (var obj in _worldObjects)
                if(obj.Point.Z == floor)
                    FloorObjects.Add(obj.View);
        }

        private void UpdateLocalizationIndicators(int floor)
        {
            if (LocalizationIndicators.Count == 0)
                return;

            double[] belief = _navigationSystem.Belief;
            int[] strides = MathHelper.GetIndexStrides(_navigationSystem.WorldShape);
            int positionCount = _navigationSystem.WorldShape[0] * _navigationSystem.WorldShape[1];
            double max = belief.Max();
            for (int i = 0; i < positionCount; i++)
                for (int a = 0; a < 4; a++)
                    LocalizationIndicators[i * 4 + a].Opacity = Math.Pow(belief[strides[1] * i + floor * strides[2] + a] / max, 0.1);
        }

        private void WorldObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFloorObjects(_floor);
        }

        private void NavigationSystem_LocalizationUpdated(object sender, NavigationSystem.LocalizationEventArgs e)
        {
            UpdateLocalizationIndicators(_floor);
        }

        private void LocalizationWindow_Closed(object sender, EventArgs e)
        {
            _localizationWindow = null;
            LocalizationIndicators.Clear();
            (sender as Window).Closed -= LocalizationWindow_Closed;
        }
    }
}