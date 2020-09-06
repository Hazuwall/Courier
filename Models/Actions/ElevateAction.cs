using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class ElevateAction : IWorldAction
    {
        public int RelativeFloor { get; set; } = 0;
        public double SuccessProb { get; set; } = 0.75;

        public bool Execute(World world, WorldObject obj)
        {
            Point oldPoint = obj.Point;
            if (world.IsNearby(oldPoint, StaticModel.ElevatorClassName))
            {
                int destFloor = ComputeProbabilisticDestinationFloor(oldPoint.Z, world.UpperBound.Z);
                Point newPoint = new Point(oldPoint.X, oldPoint.Y, destFloor);
                if (world.IsFree(newPoint))
                {
                    world.Objects.Remove(obj);
                    obj.Point = newPoint;
                    world.Objects.Add(obj);
                    return destFloor == oldPoint.Z + RelativeFloor;
                }
            }
            return false;
        }

        public int ComputeProbabilisticDestinationFloor(int currentFloor, int floorBound)
        {
            double[] distrib = new double[floorBound + 1];
            ProbabilityHelper.SetUniformDistribution(distrib);
            int expected = currentFloor + RelativeFloor;
            if (expected <= floorBound && expected >= 0)
            {
                distrib[expected] = SuccessProb;
                ProbabilityHelper.NormalizePdf(distrib, 0);
            }
            ProbabilityHelper.PdfToCdf(distrib);
            return ProbabilityHelper.Sample(distrib);
        }
    }
}
