using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class WorldGenerator
    {
        public ObjectFactory Factory { get; }
        public World World { get; }

        public WorldGenerator()
        {
            Factory = new ObjectFactory();
            World = new World(new Point(0,0,0), new Point(9, 9, 0), new List<WorldObject>());
        }

        public WorldGenerator GenerateBuilding()
        {
            for (int i = 0; i < 9; i++)
            {
                var obj = Factory.CreateWallObj(World, new Point(i, 0, 0));
                World.Objects.Add(obj);
                obj = Factory.CreateWallObj(World, new Point(1 + i, 9, 0));
                World.Objects.Add(obj);
                obj = Factory.CreateWallObj(World, new Point(0, i + 1, 0));
                World.Objects.Add(obj);
                obj = Factory.CreateWallObj(World, new Point(9, i, 0));
                World.Objects.Add(obj);
            }
            return this;
        }

        public WorldGenerator GenerateFurniture()
        {
            var obj = Factory.CreateDeskObj(World, new Point(6, 7, 0));
            //World.Objects.Add(obj);
            return this;
        }

        public WorldGenerator GenerateNPC()
        {
            var npc = Factory.CreateWalkerObj(World, new Point(6, 6, 0));
            World.Objects.Add(npc);
            return this;
        }

        public WorldGenerator GenerateRobot(out Robot robot)
        {
            var robotObj = Factory.CreateRobotObj(World, new Point(1, 1, 0));
            World.Objects.Add(robotObj);
            robot = robotObj.Model as Robot;
            return this;
        }
    }
}
