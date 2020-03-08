using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    /// <summary>
    /// Класс, включающий математические функции для работы с вероятностью
    /// </summary>
    public static class ProbabilityHelper
    {
        public const double StandartNormalDitributionFactor = 0.3989422804014326;

        private readonly static Random _Random = new Random();

        /// <summary>
        /// Генерация вектора нормального распределения
        /// </summary>
        /// <param name="mean">Математическое ожидание</param>
        /// <param name="std">Стандартное отклонение</param>
        /// <param name="returnCdf">Вернуть ли функцию распределения вероятности,
        /// иначе будет возвращена плотность распределения</param>
        /// <returns></returns>
        public static double[] NormalDistribution(double mean, double std, bool returnCdf = false)
        {
            int size = (int)((3 * std + Math.Abs(mean)) * 2 + 1);
            if (size % 2 == 0)
                size++;

            //Расчёт плотности нормального распределения
            double[] distribution = new double[size];
            int offset = size / 2;
            for(int x = 0; x < distribution.Length; x++)
            {
                double a = (x - offset - mean) / std;
                double p = StandartNormalDitributionFactor * Math.Exp(-a * a / 2) / std;
                if (p > 1)
                    p = 1;
                else if (p < double.Epsilon)
                    p = double.Epsilon;
                distribution[x] = p;
            }

            //Интегрирование для получения функции распределения
            if (returnCdf)
                PdfToCdf(distribution);
            return distribution;
        }

        public static double[,] Normal2dDistribution(double mean1, double mean2, double std1, double std2)
        {
            double[] xDistrib = NormalDistribution(mean1, std1);
            double[] yDistrib = NormalDistribution(mean2, std2);
            double[,] distrib = new double[xDistrib.Length, yDistrib.Length];

            for (int x = 0; x < xDistrib.Length; x++)
                for (int y = 0; y < yDistrib.Length; y++)
                    distrib[x, y] = xDistrib[x] * yDistrib[y];
            return distrib;
        }

        public static void PdfToCdf(double[] distribution)
        {
            for (int x = 1; x < distribution.Length; x++)
                distribution[x] += distribution[x - 1];
            distribution[distribution.Length - 1] = 1;
        }

        /// <summary>
        /// Генерация дискретной случайной величины по распределению вероятности.
        /// Случайной величиной является индекс элемента вектора
        /// </summary>
        /// <param name="cdf">Функция распределения вероятности</param>
        /// <returns></returns>
        public static int Sample(double[] cdf)
        {
            double prob = _Random.NextDouble();
            int randVar = 0;
            while (randVar < cdf.Length && prob > cdf[randVar])
                randVar++;
            return randVar;
        }

        /// <summary>
        /// Генерация случайной величины по нормальному распределению
        /// </summary>
        /// <param name="mean">Математическое ожидание</param>
        /// <param name="std">Стандартное отклонение</param>
        /// <returns></returns>
        public static int SampleFromNormal(double mean, double std)
        {
            double[] cdf = NormalDistribution(mean, std, true);
            return Sample(cdf) - cdf.Length /2;
        }

        public static void BayesTheorem(double[] prior, double[] posterior, double lowerBound=0)
        {
            double sum = 0;
            for (int i = 0; i < prior.Length; i++)
            {
                double prob = prior[i] * posterior[i];
                prior[i] = prob;
                sum += prob;
            }
            NormalizePdf(prior, lowerBound, sum);
        }

        public static void NormalizePdf(double[] pdf, double lowerBound)
        {
            double sum = pdf.Sum();
            NormalizePdf(pdf, lowerBound, sum);
        }

        public static void NormalizePdf(double[] pdf, double lowerBound, double sum)
        {
            if (sum < double.Epsilon * 1000 || double.IsNaN(sum))
                SetUniformDistribution(pdf);
            else
                for (int i = 0; i < pdf.Length; i++)
                {
                    double p = pdf[i] / sum;
                    pdf[i] = p > lowerBound ? p : lowerBound;
                }
        }

        public static void SetUniformDistribution(double[] output)
        {
            double uniformProb = 1f / output.Length;
            for (int i = 0; i < output.Length; i++)
                output[i] = uniformProb;
        }
    }
}
