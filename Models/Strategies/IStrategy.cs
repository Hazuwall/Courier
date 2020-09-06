using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public interface IStrategy{
        IWorldAction GetAction(bool isClear, bool isElevator);
    }
}
