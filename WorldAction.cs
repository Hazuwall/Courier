using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public interface IWorldAction
    {
        bool Execute(World world, WorldObject obj);
    }

    /// <summary>
    /// Действие, перемещающее объект с точностью, распределённой по нормальному закону
    /// </summary>
    public class MoveAction : IWorldAction
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
            return Probability.SampleFromNormal(Distance, std);
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

    public class LiftAction : IWorldAction
    {
        private readonly int _floor=0;
        private readonly double _prob=0;


        public bool Execute(World world, WorldObject obj)
        {
            if (obj != null && world.IsNearby(obj.Point, Lift.ClassName))
            {
                var oldPoint = obj.Point;
                double rand = new Random().NextDouble();
                if (rand > _prob)
                {
                    var newPoint = new Point(oldPoint.X, oldPoint.Y, _floor);
                    if (world.IsFree(newPoint))
                    {
                        obj.Point = newPoint;
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Действие, создающее скользкий пол под объектом
    /// </summary>
    public class MopAction : IWorldAction
    {
        public bool Execute(World world, WorldObject obj)
        {
            var point = obj.Point;
            if (world.GetWetness(point)==0)
            {
                var wetFloorObj = new ObjectFactory().CreateWetFloorObj(point);
                world.Objects.Add(wetFloorObj);
            }
            return true;
        }
    }

    /// <summary>
    /// Действие, удаляющие объект из мира
    /// </summary>
    public class RemoveAction : IWorldAction
    {
        public bool Execute(World world, WorldObject obj)
        {
            world.Objects.Remove(obj);
            return true;
        }
    }

    /// <summary>
    /// Действие, поворачивающее объект с точностью, распределённой по нормальному закону
    /// </summary>
    public class RotateAction : IWorldAction
    {
        public int Angle { get; set; } = 0;
        public double Std { get; set; } = 1;
        public double WetnessFactor { get; set; } = 1;
        public double AngleFactor { get; set; } = 1;

        public bool Execute(World world, WorldObject obj)
        {
            int wetness = world.GetWetness(obj.Point);
            var probAngle = ComputeProbabilisticAngle(wetness);

            int destOrientation = MathHelper.GetOneTurn(obj.Orientation + probAngle);
            int expected = MathHelper.GetOneTurn(obj.Orientation + Angle);
            obj.Orientation = destOrientation;
            return destOrientation == expected;
        }

        public int ComputeProbabilisticAngle(int wetness)
        {
            double std = Std * (1 + Math.Min(WetnessFactor * wetness + Math.Abs(Angle/90) * AngleFactor, 1));
            return Probability.SampleFromNormal(Angle/90, std)*90;
        }
    }
}
