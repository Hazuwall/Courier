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

        public int Orientation { get; set; }
        public string[] KnownClasses { get; }
        public double[,] ConfusionMatrix { get; }
        public double SuccessProb { get; set; } = 1;

        public Camera(World world, string[] classes, double[,] confusionMatrix)
        {
            _world = world;
            KnownClasses = classes;
            ConfusionMatrix = confusionMatrix;
        }

        public Dictionary<string,double> Measure(ModelBase model, int orientation)
        {
            double[] distrib = new double[KnownClasses.Length];

            if (new Random().NextDouble() < SuccessProb)
            {
                //Нормальная работа камеры
                var obj = _world.Find(model);
                int absOrientation = MathHelper.GetOneTurn(obj.Orientation + orientation);
                Point point = obj.Point + new Point(absOrientation);

                string seenClass = _world.FindOrDefault(point)?.Model.Class;
                if (seenClass == null)
                    seenClass = ModelBase.EmptyClassName;

                //Превращение точного прогноза в вероятностное
                int probClassIndex = GetProbabalisticClassIndex(seenClass);

                //Составление распределения вероятности по предполагаемому классу
                for (int i = 0; i < KnownClasses.Length; i++)
                    distrib[i] = ConfusionMatrix[i, probClassIndex];
                ProbabilityHelper.NormalizePdf(distrib, 0);
            }
            else
            {
                //Сбой камеры - вероятность распределяется случайно
                ProbabilityHelper.SetRandomDistribution(distrib);
            }

            //Запись предсказания в словарь
            var prediction = new Dictionary<string, double>();
            for (int i = 0; i < KnownClasses.Length; i++)
                prediction.Add(KnownClasses[i], distrib[i]);
            return prediction;
        }

        /// <summary>
        /// Получить вероятностный индекс класса по распределению из матрицы ошибок
        /// </summary>
        /// <param name="trueClassName">Истинное название класса</param>
        /// <returns></returns>
        public int GetProbabalisticClassIndex(string trueClassName)
        {
            int trueClassIndex = Array.IndexOf(KnownClasses, trueClassName);
            if (trueClassIndex == -1)
                trueClassIndex = Array.IndexOf(KnownClasses, ModelBase.UnknownClassName);

            double[] distrib = new double[KnownClasses.Length];
            for (int i = 0; i < KnownClasses.Length; i++)
                distrib[i] = ConfusionMatrix[trueClassIndex, i];
            ProbabilityHelper.NormalizePdf(distrib, 0);
            ProbabilityHelper.PdfToCdf(distrib);
            return ProbabilityHelper.Sample(distrib);
        }
    }
}
