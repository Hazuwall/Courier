using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class StaticModel : ModelBase
    {
        public override bool DoCollide => true;
        public override bool IsCallable => false;
        public override bool IsStatic => true;
        public override string Class => "Static";
    }

    public class Desk : StaticModel
    {
        public const string ClassName = "Desk";
        public override string Class => ClassName;
    }

    public class Plant : StaticModel
    {
        public const string ClassName = "Plant";
        public override string Class => ClassName;
    }

    public class Lift : StaticModel
    {
        public const string ClassName = "Lift";

        public override string Class => ClassName;

    }

    public class Window : StaticModel
    {
        public const string ClassName = "Window";
        public override string Class => ClassName;
    }

    public class Wall : StaticModel
    {
        public const string ClassName = "Wall";
        public override string Class => ClassName;
    }
}
