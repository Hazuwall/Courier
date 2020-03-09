using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Courier
{
    public class ObjectFactory
    {
        private readonly Random _random = new Random();
        private Camera _robotCamera = null;
        private Camera _walkerCamera = null;

        public ProbabilitySettings Settings { get; set; } = null;
        public double Size { get; set; } = 50;

        private Camera InitializeRobotCamera(World world)
        {
            if (Settings != null)
            {
                string[] classes = new string[] {
                    Person.ClassName,
                    StaticModel.DeskClassName,
                    StaticModel.ElevatorClassName,
                    StaticModel.PlantClassName,
                    StaticModel.WallClassName,
                    StaticModel.WindowClassName,
                    ModelBase.EmptyClassName,
                    ModelBase.UnknownClassName
                };
                return _robotCamera = new Camera(world, classes, Settings.ConfusionMatrix)
                {
                    DeviationStd = Settings.CameraDeviationStd,
                    SuccessProb = Settings.CameraSuccessStd
                };
            }
            else
            {
                string[] classes = new string[] {
                    ModelBase.EmptyClassName,
                    ModelBase.UnknownClassName
                };
                double[,] confusion = new double[,]
                {
                    { 1, 0 },
                    { 0, 1 }
                };
                return _robotCamera = new Camera(world, classes, confusion);
            }
        }

        private Camera InitializeWalkerCamera(World world)
        {
            string[] classes = new string[] {
                ModelBase.EmptyClassName,
                ModelBase.UnknownClassName
            };
            double[,] confusion = new double[,]
            {
                { 1, 0 },
                { 0, 1 }
            };
            return _walkerCamera = new Camera(world, classes, confusion)
            {
                DeviationStd = 0.0001
            };
        }

        public WorldObject CreateDeskObj(Point point)
        {
            var model = new StaticModel(StaticModel.DeskClassName);
            var obj = new WorldObject(model, Size, point, -90);
            return obj;
        }

        public WorldObject CreateElevatorObj(Point point)
        {
            var model = new StaticModel(StaticModel.ElevatorClassName);
            var obj = new WorldObject(model, Size, point);
            return obj;
        }


        public WorldObject CreatePersonObj(Point point)
        {
            var model = new Person();
            var obj = new WorldObject(model, Size, point);
            return obj;
        }

        public WorldObject CreatePlantObj(Point point)
        {
            var model = new StaticModel(StaticModel.PlantClassName);
            var obj = new WorldObject(model, Size, point);
            return obj;
        }

        public WorldObject CreateRobotObj(World world, Point point)
        {
            var camera = _robotCamera ?? InitializeRobotCamera(world);
            NavigationSystem nav;
            IStrategy strategy;
            if (Settings != null)
            {
                nav = new NavigationSystem(world)
                {
                    ElevationStd = Settings.EstimatedElevationStd,
                    RotationStd = Settings.EstimatedRotationStd,
                    TranslationStd = Settings.EstimatedTranslationStd,
                    MeasurementSuccessProb = Settings.EstimatedCameraSuccessProb
                };
                strategy = new WanderStrategy(_random)
                {
                    ElevateProb = Settings.DecideElevateProb,
                    TranslateProb = Settings.DecideTranslateProb,
                    RotateProb = Settings.DecideRotateProb,
                    IsExact = false,
                    Settings = Settings
                };
            }
            else
            {
                nav = new NavigationSystem(world);
                strategy = new WanderStrategy(_random) { IsExact = false };
            }
            var model = new Robot(camera, nav, strategy);
            int orientation = _random.Next(0, 4) * 90;
            var obj = new WorldObject(model, Size, point, orientation);
            return obj;
        }

        public WorldObject CreateWalkerObj(World world, Point point, bool isCleaner=false)
        {
            var camera = _walkerCamera ?? InitializeWalkerCamera(world);
            var behavior = new WanderStrategy(_random)
            {
                ElevateProb = 0,
                TranslateProb = 0.8,
                RotateProb = 0.1,
                IsExact = true
            };
            var model = isCleaner ? new Cleaner(camera, behavior) : new Walker(camera, behavior);
            int orientation = _random.Next(0, 4) * 90;
            var obj = new WorldObject(model, Size, point, orientation);
            return obj;
        }

        public WorldObject CreateWallObj(Point point)
        {
            var model = new StaticModel(StaticModel.WallClassName);
            var obj = new WorldObject(model, Size, point);
            return obj;
        }

        public WorldObject CreateWindowObj(Point point)
        {
            var model = new StaticModel(StaticModel.WindowClassName);
            var obj = new WorldObject(model, Size, point);
            return obj;
        }

        public WorldObject CreateWetFloorObj(Point point, int orientation=0)
        {
            int wetness = _random.Next(3, 20);
            var model = new WetFloor(wetness);
            var obj = new WorldObject(model, Size, point, orientation);
            return obj;
        }
    }
}
