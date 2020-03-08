using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public static class MathHelper
    {
        /// <summary>
        /// Привести угол к одному обороту
        /// </summary>
        /// <param name="angle">Угол в градусах</param>
        /// <returns></returns>
        public static int GetOneTurn(int angle)
        {
            int result = angle % 360;
            if (result < 0)
                result += 360;
            return result;
        }

        public static (double,double) GetIntensityVector(double weightXPos, double weightXNeg, double weightYPos, double weightYNeg)
        {
            double xCenter = weightXPos - weightXNeg;
            double yCenter = weightYPos - weightYNeg;
            double length = Math.Sqrt(xCenter * xCenter + yCenter * yCenter);
            return (xCenter / length, yCenter / length);
        }

        /// <summary>
        /// Получить коэффициенты, на которые необходимо умножить индексы многомерного
        /// массива данной формы, чтобы получить индекс аналогичного одномерного массива
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        public static int[] GetIndexStrides(int[]shape)
        {
            int[] strides = new int[shape.Length];
            int stride = 1;
            for (int i = strides.Length-1; i >= 0; i--)
            {
                strides[i] = stride;
                stride *= shape[i];
            }
            return strides;
        }

        /// <summary>
        /// Получить индексы многомерного массива по индексу аналогичного одномерного массива
        /// </summary>
        /// <param name="index"></param>
        /// <param name="shape"></param>
        /// <returns></returns>
        public static int[] GetMultiDimensionalIndices(int index, int[] shape)
        {
            int[] indices = new int[shape.Length];
            for (int i = indices.Length - 1; i >= 0; i--)
            {
                indices[i] = index % shape[i];
                index /= shape[i];
            }
            return indices;
        }

        /// <summary>
        /// Циклично сдвинуть элементы вектора на 1 вправо
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vector"></param>
        public static void RollVector<T>(T[] vector)
        {
            T temp = vector[vector.Length - 1];
            for (int i = vector.Length - 1; i > 0; i--)
                vector[i] = vector[i-1];
            vector[0] = temp;
        }

        public static void SetLowerBound(double[] arr, double bound)
        {
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] < bound)
                    arr[i] = bound;
        }

        /// <summary>
        /// Круговая одномерная транспонированная свёртка
        /// </summary>
        /// <param name="startIndex">Индекс вектора, с которого нужно начать свёртку</param>
        /// <param name="vector">Вектор, над которым производится операция</param>
        /// <param name="kernel">Ядро свёртки, длина которого соответствует длине обрабатываемой части вектора</param>
        /// <param name="output">Выходной вектор, длина которого равна длине ядра</param>
        public static void TransposedCircularConvolution1d(int startIndex, double[] vector, double[] kernel, double[] output)
        {
            Array.Clear(output, 0, output.Length);
            for (int i = 0; i < kernel.Length; i++)
                for (int j = 0; j < kernel.Length; j++)
                    output[(j + i) % kernel.Length] += vector[startIndex + i] * kernel[j];
        }

        public static void TransposedConvolution1d(double[] input, int index, int axisIndex, int axisStride, int axisLength, double[] kernel, double[] output)
        {
            int half = kernel.Length / 2;
            for (int i = Math.Max(0, half - axisIndex); i < kernel.Length && axisIndex + i - half < axisLength; i++)
            {
                int localIndex = index + (i - half) * axisStride;
                output[localIndex] += input[localIndex] * kernel[i];
            }
        }

        public static void Convolution1d(double[] A, double[] kernel, double[] output)
        {
            Array.Clear(output, 0, output.Length);
            int half = kernel.Length / 2;
            for (int x = 0; x < output.Length; x++)
                for (int i = Math.Max(0, half - x); i < kernel.Length && x + i - half < A.Length; i++)
                    output[x] += A[x + i - half] * kernel[i];
        }

        public static void Convolution2d(double[,] A, double[] kernel, double[,] output)
        {
            Array.Clear(output, 0, output.Length);
            int half = kernel.Length / 2;
            for (int x = 0; x < output.GetLength(0); x++)
                for (int y = 0; y < output.GetLength(1); y++)
                    for (int i = Math.Max(0, half - x); i < kernel.Length && x + i - half < A.GetLength(0); i++)
                            output[x, y] += A[x + i - half, y] * kernel[i];
        }
    }
}
