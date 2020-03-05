using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public static class Probability
    {
        public const double StandartNormalDitributionFactor = 0.3989422804014326;
        public const double Sqrt2 = 1.414213562373;

        public static double[] NormalPDF(double mean, double std, out int size)
        {
            size = (int)((3 * std + mean) * 2 + 1);
            if (size % 2 == 0)
                size++;
            double[] density = new double[size];
            int offset = size / 2;
            for(int x = 0; x < density.Length; x++)
            {
                double a = ((x - offset - mean) / (Sqrt2 * std));
                density[x] = StandartNormalDitributionFactor / std * Math.Exp(-a * a);
            }
            return density;
        }

        public static double[] CDF(double[] density)
        {
            double sum = 0;
            double[] cumsum = new double[density.Length];
            for (int x = 0; x < density.Length; x++)
            {
                sum += density[x];
                cumsum[x] = sum;
            }
            return cumsum;
        }

        public static int Sample(double[] cdf)
        {
            double continuousVar = new Random().NextDouble();
            int discreteVar = 0;
            while (discreteVar < cdf.Length && continuousVar > cdf[discreteVar])
                discreteVar++;
            return discreteVar;
        }

        public static int SampleFromNormal(double mean, double std)
        {
            double[] pdf = NormalPDF(mean, std, out int size);
            double[] cdf = CDF(pdf);
            return Sample(cdf) - size /2;
        }

        public static void Normalize4D(double[] density, int[]size)
        {
            int[] factors = MathHelper.GetIndexFactors(size);
            for (int axis = 3; axis >= 0; axis--)
            {
                MathHelper.RollVector(factors);
                MathHelper.RollVector(size);
                for (int i = 0; i < size[0]; i++)
                {
                    for (int j = 0; j < size[1]; j++)
                    {
                        for (int k = 0; k < size[2]; k++)
                        {
                            double sum = 0;
                            for (int x = 0; x < size[3]; x++)
                                sum += density[i * factors[0] + j * factors[1] + k * factors[2] + x * factors[3]];
                            for (int x = 0; x < size[3]; x++)
                                if(sum!=0)
                                    density[i * factors[0] + j * factors[1] + k * factors[2] + x * factors[3]] /= sum;
                        }
                    }
                }
            }
        }
    }
}
