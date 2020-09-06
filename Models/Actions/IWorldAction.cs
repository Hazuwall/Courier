using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public interface IWorldAction
    {
        bool Execute(World world, WorldObject obj);
    }
}
