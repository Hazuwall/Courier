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

    public class WanderStrategy : IStrategy
    {
        private readonly Random _random;

        public double TranslateProb { get; set; } = 0.5;
        public double RotateProb { get; set; } = 0.5;
        public double ElevatorProb { get; set; } = 0.5;
        public bool IsExact { get; set; } = true;

        public WanderStrategy(Random random)
        {
            _random = random;
        }

        public IWorldAction GetAction(bool isClear, bool isElevator)
        {
            if(isElevator && _random.NextDouble() < ElevatorProb)
            {
                int relativeFloor = _random.Next(0, 2) == 0 ? -1 : 1;
                return ActionFactory.CreateElevatorAction(relativeFloor, IsExact);
            }

            if (!isClear)
            {
                return ActionFactory.CreateRotateAction(-90, IsExact);
            }

            double randVar = _random.NextDouble();
            if (randVar < TranslateProb)
            {
                int distance = _random.Next(1, 3);
                return ActionFactory.CreateTranslationAction(distance, 0, IsExact);
            }
            else if (randVar - TranslateProb < RotateProb)
            {
                int angle = _random.Next(0, 2) == 0 ? -90 : 90;
                return ActionFactory.CreateRotateAction(angle, IsExact);
            }
            else
                return null;
        }
    }
}
