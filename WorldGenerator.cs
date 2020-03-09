﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class WorldGenerator
    {
        private readonly double _canvasSize;

        public ObjectFactory Factory { get; }
        public World World => _world;
        private World _world = null;

        public Robot Robot => _robot;
        private Robot _robot = null;

        public ProbabilitySettings Settings
        {
            get { return Factory.Settings; }
            set { Factory.Settings = value; }
        }

        public WorldGenerator(double canvasSize)
        {
            _canvasSize = canvasSize;
            Factory = new ObjectFactory();
        }

        public World Generate(string[,,] map, Collection<WorldObject> objectCollection)
        {
            Factory.Size = _canvasSize / map.GetLength(2);

            World world = new World(new Point(0, 0, 0),
                new Point(map.GetLength(2) - 1, map.GetLength(1) - 1, map.GetLength(0) - 1),
                objectCollection);

            WorldObject obj = null;
            Point point;
            bool isStatic;
            char code;
            Dictionary<Point, char> npcDictionary = new Dictionary<Point, char>();
            for (int z = 0; z < map.GetLength(0); z++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    for (int x = 0; x < map.GetLength(2); x++)
                    {
                        point = new Point(x, map.GetLength(1) - y - 1, z);
                        isStatic = true;
                        code = map[z, y, x][0];
                        switch (code)
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
                                npcDictionary.Add(point, code);
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
                    default:
                        continue;
                }
                world.Objects.Add(obj);
            }
            return _world = world;
        }

        public static string[,,] ParseMapFromJson(string path)
        {
            string rawJson = File.ReadAllText(path);
            return (string[,,])JsonConvert.DeserializeObject(rawJson, typeof(string[,,]));
        }
    }
}
