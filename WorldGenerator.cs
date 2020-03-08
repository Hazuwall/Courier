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

        public World World => _world;
        private World _world = null;

        public Robot Robot => _robot;
        private Robot _robot = null;

        public WorldGenerator()
        {
            Factory = new ObjectFactory();
        }

        public World Generate(char[,,] map)
        {
            World world = new World(new Point(0, 0, 0),
                new Point(map.GetLength(2) - 1, map.GetLength(1) - 1, map.GetLength(0) - 1),
                new List<WorldObject>());

            WorldObject obj = null;
            Point point;
            bool isStatic;
            Dictionary<Point, char> npcDictionary = new Dictionary<Point, char>();
            for (int z = 0; z < map.GetLength(0); z++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    for (int x = 0; x < map.GetLength(2); x++)
                    {
                        point = new Point(x, map.GetLength(1) - y - 1, z);
                        isStatic = true;
                        switch (map[z, y, x])
                        {
                            case 'w':
                                obj = Factory.CreateWallObj(point);
                                break;
                            case 'f':
                                obj = Factory.CreateWindowObj(point);
                                break;
                            case 'p':
                                obj = Factory.CreatePlantObj(point);
                                break;
                            case 'e':
                                obj = Factory.CreateElevatorObj(point);
                                break;
                            case 'd':
                                obj = Factory.CreateDeskObj(point);
                                break;
                            case ' ':
                                isStatic = false;
                                break;
                            default:
                                npcDictionary.Add(point, map[z, y, x]);
                                isStatic = false;
                                break;
                        }
                        if (isStatic)
                            world.Objects.Add(obj);
                    }
                }
            }

            foreach(var npcInfo in npcDictionary)
            {
                switch (npcInfo.Value)
                {
                    case 'M':
                        obj = Factory.CreatePersonObj(npcInfo.Key);
                        break;
                    case 'W':
                        obj = Factory.CreateWalkerObj(world, npcInfo.Key);
                        break;
                    case 'C':
                        obj = Factory.CreateWalkerObj(world, npcInfo.Key, isCleaner: true);
                        break;
                    case 'R':
                        obj = Factory.CreateRobotObj(world, npcInfo.Key);
                        _robot = obj.Model as Robot;
                        break;
                }
                world.Objects.Add(obj);
            }
            return _world = world;
        }

        public static char[,,] GetDefaultMap()
        {
            return new char[,,]
            {
                {
                    { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                    { 'w', ' ', ' ', ' ', ' ', 'p', ' ', 'M', 'w' },
                    { 'w', 'M', ' ', ' ', ' ', ' ', ' ', 'd', 'w' },
                    { 'p', 'd', ' ', 'p', 'p', 'p', ' ', ' ', 'e' },
                    { 'w', ' ', ' ', ' ', 'W', ' ', ' ', ' ', 'w' },
                    { 'p', ' ', ' ', 'p', 'p', 'p', ' ', ' ', 'w' },
                    { 'w', ' ', 'R', ' ', ' ', ' ', ' ', ' ', 'w' },
                    { 'w', ' ', ' ', ' ', 'p', ' ', ' ', 'C', 'w' },
                    { 'w', 'f', 'w', 'f', 'w', 'f', 'w', 'f', 'w' }
                }
            };
        }
    }
}
