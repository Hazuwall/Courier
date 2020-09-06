using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
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
            return ProbabilityHelper.SampleFromNormal(Angle/90, std)*90;
        }
    }
}
