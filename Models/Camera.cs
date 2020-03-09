using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class Camera
    {
        private readonly World _world;
        private double[] _gaussianCdf;

        public int Orientation { get; set; }
        public string[] KnownClasses { get; }
        public double[,] ConfusionMatrix { get; }
        public double DeviationStd
        {
            get { return _deviationStd; }
            set
            {
                _deviationStd = value;
                _gaussianCdf = ProbabilityHelper.NormalDistribution(0, value, true);
            }
        }
        private double _deviationStd = 1;
        public double SuccessProb { get; set; } = 1;

        public Camera(World world, string[] classes, double[,] confusionMatrix)
        {
            _world = world;
            _gaussianCdf = ProbabilityHelper.NormalDistribution(0, 1, true);
            KnownClasses = classes;
            ConfusionMatrix = confusionMatrix;
        }

        public Dictionary<string,double> Measure(ModelBase model, int orientation)
        {
            double[] distrib = new double[ConfusionMatrix.GetLength(1)];

            if (new Random().NextDouble() < SuccessProb)
            {
                //Нормальная работа камеры
                var obj = _world.Find(model);
                int absOrientation = MathHelper.GetOneTurn(obj.Orientation + orientation);
                Point point = obj.Point + new Point(absOrientation);

                string seenClass = _world.FindOrDefault(point)?.Model.Class;
                if (seenClass == null)
                    seenClass = ModelBase.EmptyClassName;
                int trueClassIndex = Array.IndexOf(KnownClasses, seenClass);
                if (trueClassIndex == -1)
                {
                    seenClass = ModelBase.UnknownClassName;
                    trueClassIndex = Array.IndexOf(KnownClasses, seenClass);
                }

                for (int i = 0; i < KnownClasses.Length; i++)
                {
                    //Случайное отклонение от матрицы, распределённое по Гауссу
                    double mean = ConfusionMatrix[trueClassIndex, i];
                    double dev = 0.1 * (ProbabilityHelper.Sample(_gaussianCdf) - _gaussianCdf.Length / 2);
                    distrib[i] = Math.Max(mean + dev, 0);
                }
                ProbabilityHelper.NormalizePdf(distrib, 0);
            }
            else
            {
                //Сбой камеры - вероятность распределяется случайно
                ProbabilityHelper.SetRandomDistribution(distrib);
            }

            var prediction = new Dictionary<string, double>();
            for (int i = 0; i < KnownClasses.Length; i++)
                prediction.Add(KnownClasses[i], distrib[i]);
            return prediction;
        }
    }
}
