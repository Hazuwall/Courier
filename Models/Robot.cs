﻿using System;
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
        private bool _isMovementNext = false;

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
            IWorldAction action = null;
            if(_isMovementNext)
            {
                var prediction = _camera.Measure(this, 0);
                action = _strategy.GetAction(prediction[EmptyClassName] > 0.7, prediction[StaticModel.ElevatorClassName] > 0.7);
                if (action is TranslationAction)
                {
                    var translation = action as TranslationAction;
                    NavigationSystem.OnTranslation(translation.Direction, translation.Distance);
                }
                else if (action is RotateAction)
                    NavigationSystem.OnRotation((action as RotateAction).Angle);
            }
            else
            {
                for(int direction=0;direction<360;direction+=90)
                {
                    var prediction = _camera.Measure(this, direction);
                    NavigationSystem.OnMeasurement(prediction, direction);
                }
            }
            _isMovementNext = !_isMovementNext;
            return action == null ? null : new List<IWorldAction>() { action };
        }
    }
}
