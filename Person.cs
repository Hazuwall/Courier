using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class Person : ModelBase
    {
        public const string ClassName = "Person";

        public override bool DoCollide => true;
        public override bool IsCallable => false;
        public override bool IsStatic => false;
        public override string Class => ClassName;
    }

    public class Walker : Person
    {
        private readonly Camera _camera;
        private readonly IStrategy _strategy;
        public override bool IsCallable => true;

        public Walker(Camera camera, IStrategy strategy)
        {
            _camera = camera;
            _strategy = strategy;
        }

        public override IEnumerable<IWorldAction> Call()
        {
            double emptyProb = _camera.Measure(this, 0)[EmptyClassName];
            var action = _strategy.GetAction(emptyProb > 0.9);
            return action == null ? null : new List<IWorldAction>() { action };
        }
    }

    public class Cleaner : Walker
    {
        public Cleaner(Camera camera, IStrategy behavior):base(camera, behavior)
        {
        }

        public override IEnumerable<IWorldAction> Call()
        {
            var actions = new List<IWorldAction>() { ActionFactory.CreateMopAction() };
            var baseActions = base.Call();
            if (baseActions != null)
                actions.AddRange(baseActions);
            return actions;
        }
    }
}
