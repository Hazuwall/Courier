using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class WanderStrategy : IStrategy
    {
        private readonly Random _random;

        public ActionFactory Factory { get; }
        public double TranslateProb { get; set; } = 0.5;
        public double RotateProb { get; set; } = 0.5;
        public double ElevateProb { get; set; } = 0.5;
        public bool IsExact { get; set; } = true;
        public ProbabilitySettings Settings
        {
            get { return Factory.Settings; }
            set { Factory.Settings = value; }
        }

        public WanderStrategy(Random random)
        {
            _random = random;
            Factory = new ActionFactory();
        }

        public IWorldAction GetAction(bool isClear, bool isElevator)
        {
            if(isElevator && _random.NextDouble() < ElevateProb)
            {
                int relativeFloor = _random.Next(0, 2) == 0 ? -1 : 1;
                return Factory.CreateElevateAction(relativeFloor, IsExact);
            }

            if (!isClear)
            {
                return Factory.CreateRotateAction(-90, IsExact);
            }

            double randVar = _random.NextDouble();
            if (randVar < TranslateProb)
            {
                int distance = _random.Next(1, 3);
                return Factory.CreateTranslationAction(distance, 0, IsExact);
            }
            else if (randVar - TranslateProb < RotateProb)
            {
                int angle = _random.Next(0, 2) == 0 ? -90 : 90;
                return Factory.CreateRotateAction(angle, IsExact);
            }
            else
                return null;
        }
    }
}
