using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    /// <summary>
    /// Действие, создающее скользкий пол под объектом
    /// </summary>
    public class MopAction : IWorldAction
    {
        public bool Execute(World world, WorldObject obj)
        {
            var point = obj.Point;
            if (world.GetWetness(point)==0)
            {
                var factory = new ObjectFactory() { Size = obj.Size };
                var wetFloorObj = factory.CreateWetFloorObj(point, obj.Orientation - 90);
                world.Objects.Add(wetFloorObj);
            }
            return true;
        }
    }
}
