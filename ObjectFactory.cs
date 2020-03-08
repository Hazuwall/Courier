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
        private readonly double _size;
        private Camera _robotCamera = null;
        private Camera _walkerCamera = null;

        public ObjectFactory(double size)
        {
            _size = size;
        }

        private Camera InitializeRobotCamera(World world)
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
            double[,] confusion = new double[,]
            {
                { 0.9, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01 },
                { 0.01, 0.9, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01 },
                { 0.01, 0.01, 0.9, 0.01, 0.01, 0.01, 0.01, 0.01 },
                { 0.01, 0.01, 0.01, 0.9, 0.01, 0.01, 0.01, 0.01 },
                { 0.01, 0.01, 0.01, 0.01, 0.9, 0.01, 0.01, 0.01 },
                { 0.01, 0.01, 0.01, 0.01, 0.01, 0.9, 0.01, 0.01 },
                { 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.9, 0.01 },
                { 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.9 }
            };
            return _robotCamera = new Camera(world, classes, confusion);
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
            return _walkerCamera = new Camera(world, classes, confusion);
        }

        public WorldObject CreateDeskObj(Point point)
        {
            var model = new StaticModel(StaticModel.DeskClassName);
            var obj = new WorldObject(model, _size, point, -90);
            return obj;
        }

        public WorldObject CreateElevatorObj(Point point)
        {
            var model = new StaticModel(StaticModel.ElevatorClassName);
            var obj = new WorldObject(model, _size, point);
            return obj;
        }


        public WorldObject CreatePersonObj(Point point)
        {
            var model = new Person();
            var obj = new WorldObject(model, _size, point);
            return obj;
        }

        public WorldObject CreatePlantObj(Point point)
        {
            var model = new StaticModel(StaticModel.PlantClassName);
            var obj = new WorldObject(model, _size, point);
            return obj;
        }

        public WorldObject CreateRobotObj(World world, Point point)
        {
            var camera = _robotCamera ?? InitializeRobotCamera(world);
            var navSystem = new NavigationSystem(world)
            {
                RotationStd = 0.3,
                TranslationStd = 0.3
            };
            var behavior = new WanderStrategy(_random)
            {
                ElevatorProb = 0.75,
                TranslateProb = 0.7,
                RotateProb = 0.2,
                IsExact = false
            };
            var model = new Robot(camera, navSystem, behavior);
            int orientation = _random.Next(0, 4) * 90;
            var obj = new WorldObject(model, _size, point, orientation);
            return obj;
        }

        public WorldObject CreateWalkerObj(World world, Point point, bool isCleaner=false)
        {
            var camera = _walkerCamera ?? InitializeWalkerCamera(world);
            var behavior = new WanderStrategy(_random)
            {
                ElevatorProb = 0,
                TranslateProb = 0.8,
                RotateProb = 0.1,
                IsExact = true
            };
            var model = isCleaner ? new Cleaner(camera, behavior) : new Walker(camera, behavior);
            int orientation = _random.Next(0, 4) * 90;
            var obj = new WorldObject(model, _size, point, orientation);
            return obj;
        }

        public WorldObject CreateWallObj(Point point)
        {
            var model = new StaticModel(StaticModel.WallClassName);
            var obj = new WorldObject(model, _size, point);
            return obj;
        }

        public WorldObject CreateWindowObj(Point point)
        {
            var model = new StaticModel(StaticModel.WindowClassName);
            var obj = new WorldObject(model, _size, point);
            return obj;
        }

        public WorldObject CreateWetFloorObj(Point point, int orientation=0)
        {
            int wetness = _random.Next(3, 20);
            var model = new WetFloor(wetness);
            var obj = new WorldObject(model, _size, point, orientation);
            return obj;
        }
    }
}
