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
            _tempBelief = new double[length];
            _viewMap = new int[length];
            _occupancyMap = new int[length / 4];

            //Инициализация убеждения равномерным распределением
            double uniformProb = 1f / length;
            for (int i = 0; i < _belief.Length; i++)
                _belief[i] = uniformProb;
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
            for (int i = 0; i < prior.Length; i++)
                prior[i] /= sum;
        }

        public void OnTranslation(int direction, int distance)
        {
            double[,] gaussianKernel = ProbabilityHelper.Normal2dDistribution(0, 0, 0.3, 0.3);

            int kernelSize = (distance + 1) * 3;
            if (kernelSize % 2 == 0)
                kernelSize++;
            int halfKernel = kernelSize / 2;
            double[,] directionPdf = new double[kernelSize, kernelSize];
            double[,] kernel = new double[kernelSize, kernelSize];

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
                        
                        directionPdf[distance + halfKernel, halfKernel] = _belief[index];
                        directionPdf[halfKernel, distance + halfKernel] = _belief[index+1];
                        directionPdf[-distance + halfKernel, halfKernel] = _belief[index+2];
                        directionPdf[halfKernel, -distance + halfKernel] = _belief[index+3];
                        var dirVector = MathHelper.GetIntensityVector(_belief[index], _belief[index + 2],
                            _belief[index + 1], _belief[index + 3]);

                        Array.Clear(kernel, 0, kernel.Length);
                        MathHelper.Convolution2d(directionPdf, gaussianKernel, kernel);
                        for (int i = Math.Max(0, halfKernel - x); i < kernel.GetLength(0) && x + i - halfKernel < WorldShape[0]; i++)
                        {
                            for (int j = Math.Max(0, halfKernel - y); j < kernel.GetLength(1) && y + j - halfKernel < WorldShape[1]; j++)
                            {
                                int localIndex = (x + i - halfKernel) * factors[0] + (y + j - halfKernel) * factors[1] + z * factors[2];
                                var localDirVector = MathHelper.GetIntensityVector(_belief[localIndex], _belief[localIndex + 2],
                                    _belief[localIndex + 1], _belief[localIndex + 3]);
                                for (int a1 = 0; a1 < 4; a1++)
                                {
                                    /*for (int a2 = 0; a2 < 4; a2++)
                                    {
                                        int sign = Math.Abs(a1 - a2) == 2 ? -1 : 1;
                                        if (Math.Abs(a1 - a2) == 1)
                                            continue;*/
                                        _tempBelief[localIndex + a1] += _belief[index + a1] * kernel[i, j];
                                    //}
                                }
                            }
                        }
                            
                    }
                }
            }
            _tempBelief.CopyTo(_belief, 0);
            ProbabilityHelper.NormalizePdf(_belief);
            ProbabilityChanged.Invoke(this, new LocalizationEventArgs() {
                TranslationKernel = kernel
            });
        }

        public void OnRotation(int angle)
        {
            double[] rotationPdf = ProbabilityHelper.NormalDistribution(-angle/90, 0.3);
            int offset = rotationPdf.Length / 2;
            double[] kernel = new double[4];
            for (int i = 0; i < rotationPdf.Length; i++)
                kernel[MathHelper.GetOneTurn((i-offset)*90) / 90] = rotationPdf[i];

            double[] temp = new double[4];
            for (int i = 0; i < _tempBelief.Length; i += 4)
            {
                if (_occupancyMap[i/4] != 0)
                    continue;

                MathHelper.CircularConvolution(i, _belief, kernel, temp);
                for (int j = 0; j < 4; j++)
                    _belief[i + j] = temp[j];
            }
            ProbabilityHelper.NormalizePdf(_belief);
            ProbabilityChanged.Invoke(this, new LocalizationEventArgs()
            {
                RotationKernel = kernel
            });
        }

        public void OnMeasurement(Dictionary<string,double> prediction)
        {
            double[] probs = PredictionToProbabilities(prediction);

            Array.Clear(_tempBelief,0,_tempBelief.Length);
            for(int i=0;i<_viewMap.Length;i++)
            {
                int labelIndex = _viewMap[i];
                _tempBelief[i] = probs[labelIndex] / _labelCount[labelIndex];
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
                if(labelIndex != -1)
                    probs[labelIndex] = item.Value;
            }
            return probs;
        }
    }
}
