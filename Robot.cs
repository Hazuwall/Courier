using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class Robot : ModelBase
    {
        private readonly Camera _camera;
        private readonly IStrategy _strategy;

        public NavigationSystem NavigationSystem { get; }
        public override bool DoCollide => true;
        public override bool IsCallable => true;
        public override bool IsStatic => false;
        public override string Class => "Robot";

        public Robot(Camera camera, NavigationSystem navigationSystem, IStrategy strategy)
        {
            _camera = camera;
            _strategy = strategy;
            NavigationSystem = navigationSystem;
        }

        public override IEnumerable<IWorldAction> Call()
        {
            var probs = _camera.Measure(this, 180);
            NavigationSystem.OnMeasurement(probs, 180);
            probs = _camera.Measure(this, 0);
            NavigationSystem.OnMeasurement(probs, 0);
            var action = _strategy.GetAction(probs[EmptyClassName] > 0.7);
            if (action is TranslationAction) {
                var translation = action as TranslationAction;
                NavigationSystem.OnTranslation(translation.Direction, translation.Distance);
            }
            else if (action is RotateAction)
                NavigationSystem.OnRotation((action as RotateAction).Angle);
            
            return action == null ? null : new List<IWorldAction>() { action };
        }
    }
}
