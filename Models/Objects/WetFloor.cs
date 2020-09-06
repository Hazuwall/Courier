using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class WetFloor : ModelBase
    {
        public const string ClassName = "WetFloor";

        private int _wetness;
        public int Wetness => _wetness;

        public override bool DoCollide => false;
        public override bool IsCallable => true;
        public override bool IsStatic => false;
        public override string Class => ClassName;

        public WetFloor(int wetness)
        {
            if (wetness < 0)
                throw new ArgumentException();

            _wetness = wetness;
        }
        
        public override IEnumerable<IWorldAction> Call()
        {
            if (--_wetness <= 0)
                return new IWorldAction[] { new ActionFactory().CreateRemoveAction() };
            else
                return null;
        }
    }
}
