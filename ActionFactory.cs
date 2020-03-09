using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class ActionFactory
    {
        public ProbabilitySettings Settings { get; set; }

        public ElevateAction CreateElevateAction(int relativeFloor, bool exact=true)
        {
            var action = new ElevateAction() { RelativeFloor = relativeFloor };
            if (exact)
                action.SuccessProb = 1;
            else if (Settings != null)
                action.SuccessProb = Settings.ElevationSuccessProb;
            return action;
        }

        public MopAction CreateMopAction()
        {
            return new MopAction();
        }

        public TranslateAction CreateTranslationAction(int distance, int direction, bool exact = true)
        {
            var action = new TranslateAction()
            {
                Distance = distance,
                Direction = direction
            };
            if(Settings != null)
            {
                action.Std = Settings.TranslationStd;
                action.WetnessFactor = Settings.TranslationWetnessFactor;
                action.DistanceFactor = Settings.TranslationDistanceFactor;
            }
            if (exact)
                action.Std = 0.00001;
            return action;
        }

        public RemoveAction CreateRemoveAction()
        {
            return new RemoveAction();
        }

        public RotateAction CreateRotateAction(int angle, bool exact=true)
        {
            var action = new RotateAction()
            {
                Angle = angle
            };
            if (Settings != null)
            {
                action.Std = Settings.RotationStd;
                action.WetnessFactor = Settings.RotationWetnessFactor;
                action.AngleFactor = Settings.RotationAngleFactor;
            }
            if (exact)
                action.Std = 0.00001;
            return action;
        }
    }
}
