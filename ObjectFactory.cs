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

        private Camera InitializeRobotCamera(World world)
        {
            string[] classes = new string[] {
                Person.ClassName,
                Wall.ClassName,
                Desk.ClassName,
                ModelBase.EmptyClassName,
                ModelBase.UnknownClassName
            };
            double[,] confusion = new double[,]
            {
                { 0.8, 0.05, 0.05, 0.05, 0.05 },
                { 0.05, 0.8, 0.05, 0.05, 0.05 },
                { 0.05, 0.05, 0.8, 0.05, 0.05 },
                { 0.05, 0.05, 0.05, 0.8, 0.05 },
                { 0.05, 0.05, 0.05, 0.05, 0.8 }
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

        public WorldObject CreateDeskObj(World world, Point point)
        {
            var model = new Desk();
            var obj = new WorldObject(model, point, -90);
            return obj;
        }


        public WorldObject CreatePersonObj(Point point)
        {
            var model = new Person();
            var obj = new WorldObject(model, point);
            return obj;
        }

        public WorldObject CreateRobotObj(World world, Point point)
        {
            var camera = _robotCamera ?? InitializeRobotCamera(world);
            var navSystem = new NavigationSystem(world);
            var behavior = new WanderStrategy(_random)
            {
                Translaterob = 0.5,
                RotateProb = 0.25
            };
            var model = new Robot(camera, navSystem, behavior);
            int orientation = _random.Next(0, 4) * 90;
            var obj = new WorldObject(model, point, orientation);
            return obj;
        }

        public WorldObject CreateWalkerObj(World world, Point point, bool isCleaner=false)
        {
            var camera = _walkerCamera ?? InitializeWalkerCamera(world);
            var behavior = new WanderStrategy(_random)
            {
                Translaterob = 0.7,
                RotateProb = 0.2
            };
            var model = isCleaner ? new Cleaner(camera, behavior) : new Walker(camera, behavior);
            int orientation = _random.Next(0, 4) * 90;
            var obj = new WorldObject(model, point, orientation);
            return obj;
        }

        public WorldObject CreateWallObj(World world, Point point)
        {
            var model = new Wall();
            var obj = new WorldObject(model, point, -90);
            return obj;
        }

        public WorldObject CreateWetFloorObj(Point point)
        {
            int wetness = _random.Next(3, 15);
            var model = new WetFloor(wetness);
            var obj = new WorldObject(model, point);
            return obj;
        }
    }
}
