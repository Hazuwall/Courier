using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Courier
{
    public class Engine
    {
        public World World { get; }
        public int DelayTime { get; set; } = 1000;

        public Engine(World world)
        {
            World = world;
        }

        public async Task SimulateAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Step();
                await Task.Delay(DelayTime);
            }
        }

        public void Step()
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
        }
    }
}
