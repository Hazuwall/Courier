﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Courier
{
    public class Simulation
    {
        public World World { get; }
        public Canvas Canvas { get; }

        public Simulation(World world)
        {
            World = world;
        }

        public async Task SimulateAsync()
        {
            for (int t = 0; t < 1000; t++)
            {
                var objects = World.Objects;
                for (int i = 0; i < objects.Count; i++)
                {
                    var obj = objects[i];
                    if (obj.Model.IsCallable)
                    {
                        var actions = obj.Model.Call();
                        if (actions != null)
                            foreach (var action in actions)
                                action.Execute(World, obj);
                    }
                }
                await Task.Delay(500);
            }
        }
    }
}