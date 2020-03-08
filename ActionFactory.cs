using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public static class ActionFactory
    {
        public static ElevatorAction CreateElevatorAction(int relativeFloor, bool exact=true)
        {
            return new ElevatorAction()
            {
                RelativeFloor = relativeFloor,
                SuccessProb = exact ? 1 : 0.75
            };
        }

        public static MopAction CreateMopAction()
        {
            return new MopAction();
        }

        public static TranslationAction CreateTranslationAction(int distance, int direction, bool exact = true)
        {
            var action = new TranslationAction()
            {
                Distance = distance,
                Direction = direction,
                Std = exact ? 0.00001 : 0.3,
                WetnessFactor = 0.1,
                DistanceFactor = 0.5
            };
            return action;
        }

        public static RemoveAction CreateRemoveAction()
        {
            return new RemoveAction();
        }

        public static RotateAction CreateRotateAction(int angle, bool exact=true)
        {
            var action = new RotateAction()
            {
                Angle = angle,
                Std = exact? 0.00001 : 0.3,
                WetnessFactor = 0.1,
                AngleFactor = 0.5
            };
            return action;
        }
    }
}
