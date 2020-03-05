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

        public double MoveProb { get; set; } = 0.5;
        public double RotateProb { get; set; } = 0.5;

        public WanderStrategy(Random random)
        {
            _random = random;
        }

        public IWorldAction GetAction(bool isClear)
        {
            double randVar = _random.NextDouble();
            IWorldAction action = null;
            if (randVar < MoveProb)
            {
                if (isClear)
                {
                    int distance = _random.Next(1, 3);
                    action = ActionFactory.CreateMoveAction(distance, 0);
                }
                else
                    action = ActionFactory.CreateRotateAction(2);
            }
            else if (randVar - MoveProb < RotateProb)
            {
                int angle = _random.Next(0, 2) == 0 ? -90 : 90;
                action = ActionFactory.CreateRotateAction(angle);
            }
            return action;
        }
    }
}
