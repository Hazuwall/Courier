using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public interface IStrategy{
        IWorldAction GetAction(bool isClear);
    }

    public class WanderStrategy : IStrategy
    {
        private readonly Random _random;

        public double Translaterob { get; set; } = 0.5;
        public double RotateProb { get; set; } = 0.5;

        public WanderStrategy(Random random)
        {
            _random = random;
        }

        public IWorldAction GetAction(bool isClear)
        {
            double randVar = _random.NextDouble();
            IWorldAction action = null;
            if (randVar < Translaterob && isClear)
            {
                int distance = _random.Next(1, 3);
                action = ActionFactory.CreateTranslationAction(distance, 0);
            }
            else if (randVar - Translaterob < RotateProb)
            {
                int angle = _random.Next(0, 2) == 0 ? -90 : 90;
                action = ActionFactory.CreateRotateAction(angle);
            }
            return action;
        }
    }
}
