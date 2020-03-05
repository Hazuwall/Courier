using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public static class MathHelper
    {
        public static int GetOneTurn(int angle)
        {
            int result = angle % 360;
            if (result < 0)
                result += 360;
            return result;
        }

        public static int[] GetIndexFactors(int[]size)
        {
            int[] factors = new int[4];
            int factor = 1;
            for (int i = 3; i >= 0; i--)
            {
                factors[i] = factor;
                factor *= size[i];
            }
            return factors;
        }

        public static void RollVector<T>(T[] vector)
        {
            T temp = vector[vector.Length - 1];
            for (int i = vector.Length - 1; i > 0; i--)
                vector[i] = vector[i-1];
            vector[0] = temp;
        }
    }
}
