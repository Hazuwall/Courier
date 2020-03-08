using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class NavigationSystem
    {
        public class LocalizationEventArgs : EventArgs
        {
            public double[,] TranslationKernel { get; set; }
            public double[] RotationKernel { get; set; }
        }

        private const string ForbiddenStateLabel = "_Forbidden";

        /// <summary>
        /// Карта обзора, хранящая информацию о том, на объект какого класса
        /// падает вид из каждого состояния
        /// </summary>
        private readonly int[] _viewMap;
        /// <summary>
        /// Карта заполненности, хранящая информацию о том, объект какого класса
        /// помещён в каждую позицию
        /// </summary>
        private readonly int[] _occupancyMap;
        /// <summary>
        /// Словарь меток, связывающий мнемонический обозначения с индексами, хранящимися в картах
        /// </summary>
        private string[] _mapLabels = null;
        /// <summary>
        /// Количество состояний карты обзора с меткой, соответствующей индексу массива
        /// </summary>
        private int[] _labelCount = null;
        private readonly double[] _tempBelief;
        private readonly double[] _gaussianKernel;

        /// <summary>
        /// Форма мира
        /// </summary>
        public int[] WorldShape { get; }
        /// <summary>
        /// Убеждение системы о текущем положении
        /// </summary>
        public double[] Belief => _belief;
        private readonly double[] _belief;
        public event EventHandler<LocalizationEventArgs> ProbabilityChanged = delegate { };

        public NavigationSystem(World world)
        {
            Point upper = world.UpperBound;
            WorldShape = new int[] { upper.X + 1, upper.Y + 1, upper.Z + 1, 4 };

            int length = WorldShape[0] * WorldShape[1] * WorldShape[2] * WorldShape[3];
            _belief = new double[length];
            _viewMap = new int[length];
            _occupancyMap = new int[length / 4];

            _tempBelief = new double[length];
            _gaussianKernel = ProbabilityHelper.NormalDistribution(0, 0.25);

            //Инициализация убеждения равномерным распределением
            ProbabilityHelper.SetUniformDistribution(_belief);
            FillMaps(world);
        }

        /// <summary>
        /// Заполнить карты обзора и заполнения статическими объектами из данного мира
        /// </summary>
        /// <param name="world">Мир с нижней границей в начале координат</param>
        private void FillMaps(World world)
        {
            Array.Clear(_occupancyMap, 0, _occupancyMap.Length);
            Array.Clear(_viewMap, 0, _viewMap.Length);
            
            /*Метка пустого класса находится на первом месте в списке,
            так как карта изначально заполнена нулями*/
            List<string> mapLabels = new List<string>()
            {
                ModelBase.EmptyClassName,
                ForbiddenStateLabel
            };

            //Множители при параметрах полного состояния и позиции без ориентации соответственно
            //Они используются для расчёта индекса соответствующего элемента массива
            int[] factors = MathHelper.GetOneDimensionalIndexFactors(WorldShape);
            int[] posFactors = new int[]
            {
                factors[0] / 4,
                factors[1] / 4,
                1
            };

            //Занесение всех статических объектов, с которыми можно столкнуться, в карты
            foreach (var obj in world.Objects)
            {
                ModelBase model = obj.Model;
                if (model.IsStatic && model.DoCollide)
                {
                    string labelName = model.Class;
                    int labelIndex = mapLabels.IndexOf(labelName);
                    if (labelIndex == -1)
                    {
                        mapLabels.Add(labelName);
                        labelIndex = mapLabels.Count - 1;
                    }
                    
                    Point p = obj.Point;
                    _occupancyMap[p.X * posFactors[0] + p.Y * posFactors[1] + p.Z] = labelIndex;
                    AddViewsToMap(p.X, p.Y, p.Z, labelIndex,factors);
                }
            }

            //Занесение пустых позиций в карту обзора
            for (int i = 0; i < _occupancyMap.Length; i++)
            {
                if (_occupancyMap[i] == 0)
                {
                    int[] indices = MathHelper.GetMultiDimensionalIndices(i * 4, WorldShape);
                    AddViewsToMap(indices[0], indices[1], indices[2], 0, factors);
                }
            }

            //Сохранение словаря меток в виде массива
            _mapLabels = mapLabels.ToArray();
            _labelCount = new int[_mapLabels.Length];

            //Пометка всех запрещённых состояний в карте обзора и подсчёт меток
            int forbiddenIndex = Array.IndexOf(_mapLabels, ForbiddenStateLabel);
            for (int i = 0; i < _occupancyMap.Length; i++)
            {
                if (_occupancyMap[i] != 0)
                    for (int j = 0; j < 4; j++)
                        _viewMap[i * 4 + j] = forbiddenIndex;
                for (int j = 0; j < 4; j++) {
                    int labelIndex = _viewMap[i * 4 + j];
                    _labelCount[labelIndex]++;
                }
            }
        }

        /// <summary>
        /// Добавить окрестность точки в карту обзора
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="labelIndex"></param>
        /// <param name="factors"></param>
        private void AddViewsToMap(int x, int y, int z, int labelIndex, int[]factors)
        {
            x -= 1;
            if (x >= 0)
                _viewMap[x * factors[0] + y * factors[1] + z * factors[2]] = labelIndex;
            x += 2;
            if (x < WorldShape[0])
                _viewMap[x * factors[0] + y * factors[1] + z * factors[2] + 2] = labelIndex;
            x -= 1;
            y -= 1;
            if (y >= 0)
                _viewMap[x * factors[0] + y * factors[1] + z * factors[2] + 1] = labelIndex;
            y += 2;
            if (y < WorldShape[1])
                _viewMap[x * factors[0] + y * factors[1] + z * factors[2] + 3] = labelIndex;
        }

        public static void RecalculateProbs(double[] prior, double[] posterior)
        {
            double sum = 0;
            for (int i = 0; i < prior.Length; i++)
            {
                double prob = prior[i] * posterior[i];
                prior[i] = prob;
                sum += prob;
            }
            ProbabilityHelper.NormalizePdf(prior, sum);
        }

        public void OnTranslation(int direction, int distance)
        {
            int kernelSize = (distance + 1) * 3;
            if (kernelSize % 2 == 0)
                kernelSize++;
            int halfKernel = kernelSize / 2;
            double[,] tempKernel = new double[kernelSize, 2];
            double[,] kernel = new double[kernelSize, 2];

            Array.Clear(_tempBelief, 0, _tempBelief.Length);
            int[] factors = MathHelper.GetOneDimensionalIndexFactors(WorldShape);
            for (int x = 0; x < WorldShape[0]; x++)
            {
                for (int y = 0; y < WorldShape[1]; y++)
                {
                    for (int z = 0; z < WorldShape[2]; z++)
                    {
                        int index = x * factors[0] + y * factors[1] + z * factors[2];
                        if (_occupancyMap[index / 4] != 0)
                            continue;

                        ComputeTranslationKernel(index, distance, tempKernel, kernel);

                        for (int i = Math.Max(0, halfKernel - x); i < kernel.GetLength(0) && x + i - halfKernel < WorldShape[0]; i++)
                        {
                            int localIndex = (x + i - halfKernel) * factors[0] + y * factors[1] + z * factors[2];
                            if (_occupancyMap[localIndex / 4] != 0)
                                continue;
                            int a = i < halfKernel ? 2 : 0;

                            _tempBelief[localIndex + a] += _belief[index + a] * kernel[i, 0];
                        }
                        for (int i = Math.Max(0, halfKernel - y); i < kernel.GetLength(0) && y + i - halfKernel < WorldShape[1]; i++)
                        {
                            int localIndex = x * factors[0] + (y + i - halfKernel) * factors[1] + z * factors[2];
                            if (_occupancyMap[localIndex / 4] != 0)
                                continue;
                            int a = i < halfKernel ? 3 : 1;

                            _tempBelief[localIndex + a] += _belief[index + a] * kernel[i, 1];
                        }
                    }
                }
            }
            _tempBelief.CopyTo(_belief, 0);
            //ProbabilityHelper.NormalizePdf(_belief);
            ProbabilityChanged.Invoke(this, new LocalizationEventArgs() {
                TranslationKernel = kernel
            });
        }

        private void ComputeTranslationKernel(int posIndex, int distance, double[,] temp, double[,] output)
        {
            int half = output.Length / 4;
            temp[distance + half, 0] = _belief[posIndex];
            temp[-distance + half, 0] = _belief[posIndex + 2];
            temp[distance + half, 1] = _belief[posIndex + 1];
            temp[-distance + half, 1] = _belief[posIndex + 3];
            MathHelper.Convolution2d(temp, _gaussianKernel, output);
        }

        public void OnRotation(int angle)
        {
            double[] rotationPdf = ProbabilityHelper.NormalDistribution(angle/90, 0.25);
            int offset = rotationPdf.Length / 2;
            double[] kernel = new double[4];
            for (int i = 0; i < rotationPdf.Length; i++)
                kernel[MathHelper.GetOneTurn((i-offset)*90) / 90] = rotationPdf[i];

            double[] temp = new double[4];
            for (int i = 0; i < _tempBelief.Length; i += 4)
            {
                if (_occupancyMap[i/4] != 0)
                    continue;

                MathHelper.TransposedCircularConvolution(i, _belief, kernel, temp);
                for (int a = 0; a < 4; a++)
                    _belief[i + a] = temp[a];
            }
            //ProbabilityHelper.NormalizePdf(_belief);
            ProbabilityChanged.Invoke(this, new LocalizationEventArgs()
            {
                RotationKernel = kernel
            });
        }

        public void OnMeasurement(Dictionary<string,double> prediction, int oneTurnDirection)
        {
            double[] probs = PredictionToProbabilities(prediction);
            int offset = oneTurnDirection / 90;

            Array.Clear(_tempBelief,0,_tempBelief.Length);
            for(int i=0;i<_viewMap.Length;i+=4)
            {
                for (int a = 0; a < 4; a++)
                {
                    int labelIndex = _viewMap[i + ((a+offset)%4)];
                    _tempBelief[i+a] = probs[labelIndex] / _labelCount[labelIndex];
                }
            }

            RecalculateProbs(_belief, _tempBelief);
            ProbabilityChanged.Invoke(this, new LocalizationEventArgs());
        }

        private double[] PredictionToProbabilities(Dictionary<string, double> prediction)
        {
            double[] probs = new double[_mapLabels.Length];
            foreach(var item in prediction)
            {
                int labelIndex = Array.IndexOf(_mapLabels, item.Key);
                if (labelIndex != -1)
                    probs[labelIndex] = item.Value;
                else
                    probs[0] = item.Value;
            }
            return probs;
        }
    }
}
