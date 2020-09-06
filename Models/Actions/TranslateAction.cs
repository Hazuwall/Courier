using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    /// <summary>
    /// Действие, перемещающее объект с точностью, распределённой по нормальному закону
    /// </summary>
    public class TranslateAction : IWorldAction
    {
        public int Distance { get; set; } = 1;
        public int Direction { get; set; } = 0;
        public double Std { get; set; } = 1;
        public double WetnessFactor { get; set; } = 1;
        public double DistanceFactor { get; set; } = 1;

        public bool Execute(World world, WorldObject obj)
        {
            Point originalPoint = obj.Point;

            int wetness = world.GetWetness(originalPoint);
            var probDistance = ComputeProbabilisticDistance(wetness);

            int absDirection = MathHelper.GetOneTurn(obj.Orientation + Direction);
            Point destPoint = GetDestinationPoint(world, originalPoint, probDistance, absDirection);
            obj.Point = destPoint;

            Point expected = originalPoint + (new Point(absDirection) * Distance);
            return destPoint == expected;
        }

        public int ComputeProbabilisticDistance(int wetness)
        {
            double std = Std * (1 + Math.Min(WetnessFactor * wetness + Distance * DistanceFactor, 1));
            return ProbabilityHelper.SampleFromNormal(Distance, std);
        }

        public Point GetDestinationPoint(World world, Point originalPoint, int distance, int absDirection)
        {
            Point pointDirection = new Point(absDirection);
            Point destPoint = originalPoint + pointDirection * distance;
            var area = world.GetArea(originalPoint, destPoint);
            var nearestObj = area.FindNearestOrDefault(originalPoint);
            if (nearestObj != null)
                destPoint = nearestObj.Point - pointDirection;
            else if (!area.IsInside(destPoint))
                throw new IndexOutOfRangeException();
            return destPoint;
        }
    }
}
