using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class StaticModel : ModelBase
    {
        public const string DeskClassName = "Desk";
        public const string ElevatorClassName = "Elevator";
        public const string PlantClassName = "Plant";
        public const string WallClassName = "Wall";
        public const string WindowClassName = "Window";

        public override bool DoCollide => true;
        public override bool IsCallable => false;
        public override bool IsStatic => true;
        public override string Class => _className;
        private readonly string _className;

        public StaticModel(string className)
        {
            _className = className;
        }
    }
}
