using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    /// <summary>
    /// Действие, удаляющее объект из мира
    /// </summary>
    public class RemoveAction : IWorldAction
    {
        public bool Execute(World world, WorldObject obj)
        {
            world.Objects.Remove(obj);
            return true;
        }
    }
}
