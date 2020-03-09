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
            public double[] Belief { get; set; }
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
        private readonly double[] _tempPdf;
        private double[] _gaussianKernel;
        private readonly int[] _strides;

        /// <summary>
        /// Форма мира
        /// </summary>
        public int[] WorldShape { get; }
        /// <summary>
        /// Убеждение системы о текущем положении
        /// </summary>
        public double[] Belief => _belief;
        private readonly double[] _belief;
        public const double MinProbability = 0.0000000001;
        public event EventHandler<LocalizationEventArgs> LocalizationUpdated = delegate { };
        public double ElevationStd { get; set; } = 1;
        public double TranslationStd
        {
            get { return _translationStd; }
            set
            {
                _translationStd = value;
                _gaussianKernel = ProbabilityHelper.NormalDistribution(0, value);
            }
        }
        private double _translationStd = 1;
        public double RotationStd { get; set; } = 1;
        public double MeasurementSuccessProb { get; set; } = 1;

        public NavigationSystem(World world)
        {
            Point upper = world.UpperBound;
            WorldShape = new int[] { upper.X + 1, upper.Y + 1, upper.Z + 1, 4 };
            _strides = MathHelper.GetIndexStrides(WorldShape);

            int length = WorldShape[0] * WorldShape[1] * WorldShape[2] * WorldShape[3];
            _belief = new double[length];
            _viewMap = new int[length];
            _occupancyMap = new int[length / 4];

            _tempPdf = new double[length];
            _gaussianKernel = ProbabilityHelper.NormalDistribution(0, _translationStd);

            //Инициализация убеждения равномерным распределением
            ProbabilityHelper.SetUniformDistribution(_belief);
            FillMaps(world);
        }

        public bool IsFree(int index)
        {
            return _occupancyMap[index / 4] == 0;
        }

        #region Заполнение карты

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
                    _occupancyMap[(p.X * _strides[0] + p.Y * _strides[1] + p.Z * _strides[2]) / 4] = labelIndex;
                    AddViewsToMap(p.X, p.Y, p.Z, labelIndex,_strides);
                }
            }

            //Занесение пустых позиций в карту обзора
            for (int i = 0; i < _occupancyMap.Length; i++)
            {
                if (_occupancyMap[i] == 0)
                {
                    int[] indices = MathHelper.GetMultiDimensionalIndices(i * 4, WorldShape);
                    AddViewsToMap(indices[0], indices[1], indices[2], 0, _strides);
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
        #endregion

        #region Пересчёт вероятности

        public void OnTranslation(int direction, int distance)
        {
            int kernelSize = (distance + 1) * 3;
            if (kernelSize % 2 == 0)
                kernelSize++;

            double[] tempKernel = new double[kernelSize];
            double[] kernel = new double[kernelSize];

            Array.Clear(_tempPdf, 0, _tempPdf.Length);
            for (int index = 0; index < _belief.Length; index+=4)
            {
                if (!IsFree(index))
                    continue;
                int[] indices = MathHelper.GetMultiDimensionalIndices(index, WorldShape);
                for(int j = 0; j < 2; j++)
                {
                    ComputeTranslationKernel(index+j, distance, tempKernel, kernel);
                    TransposedConvolutionTillOccupancy(index+j, j, indices[j], kernel, _tempPdf);
                }
            }
            _tempPdf.CopyTo(_belief, 0);

            MathHelper.SetLowerBound(_belief, MinProbability);
            LocalizationUpdated.Invoke(this, new LocalizationEventArgs() {
                Belief = _belief
            });;
        }

        private void ComputeTranslationKernel(int posIndex, int distance, double[] temp, double[] output)
        {
            int half = output.Length / 2;
            temp[distance + half] = _belief[posIndex];
            temp[-distance + half] = _belief[posIndex + 2];
            MathHelper.Convolution1d(temp, _gaussianKernel, output);
        }

        private void TransposedConvolutionTillOccupancy(int posIndex, int axis, int axisIndex, double[] kernel, double[] output)
        {
            int half = kernel.Length / 2;
            for (int i = half; i < kernel.Length && axisIndex + i - half < WorldShape[axis]; i++)
            {
                int localIndex = posIndex + (i - half) * _strides[axis];
                if (!IsFree(localIndex))
                    break;
                output[localIndex] += _belief[posIndex] * kernel[i];
            }
            int lower = Math.Max(0, half - axisIndex);
            for (int i = half - 1; i >= lower; i--)
            {
                int localIndex = posIndex + (i - half) * _strides[axis] + 2;
                if (!IsFree(localIndex))
                    break;
                output[localIndex] += _belief[posIndex + 2] * kernel[i];
            }
        }

        public void OnElevation(int relativeFloor)
        {
            double[] kernel = ProbabilityHelper.NormalDistribution(relativeFloor, ElevationStd);
            int halfKernel = kernel.Length / 2;

            Array.Clear(_tempPdf, 0, _tempPdf.Length);
            int elevatorLabelIndex = Array.IndexOf(_mapLabels, StaticModel.ElevatorClassName);
            for (int index = 0; index < _viewMap.Length; index++)
            {
                if(_viewMap[index] == elevatorLabelIndex)
                {
                    int z = MathHelper.GetMultiDimensionalIndices(index, WorldShape)[2];
                    MathHelper.TransposedConvolution1d(_belief, index, z, _strides[2],
                        WorldShape[2], kernel, _tempPdf);
                }
            }

            _tempPdf.CopyTo(_belief, 0);
            MathHelper.SetLowerBound(_belief, MinProbability);
            LocalizationUpdated.Invoke(this, new LocalizationEventArgs()
            {
                Belief = _belief
            });
        }

        public void OnRotation(int angle)
        {
            double[] rotationPdf = ProbabilityHelper.NormalDistribution(angle/90, RotationStd);
            int offset = rotationPdf.Length / 2;
            double[] kernel = new double[4];
            for (int i = 0; i < rotationPdf.Length; i++)
                kernel[MathHelper.GetOneTurn((i-offset)*90) / 90] = rotationPdf[i];

            double[] temp = new double[4];
            for (int index = 0; index < _belief.Length; index += 4)
            {
                if (!IsFree(index))
                    continue;
                MathHelper.TransposedCircularConvolution1d(index, _belief, kernel, temp);
                temp.CopyTo(_belief, index);
            }

            MathHelper.SetLowerBound(_belief, MinProbability);
            LocalizationUpdated.Invoke(this, new LocalizationEventArgs()
            {
                Belief = _belief
            });
        }

        public void OnMeasurement(Dictionary<string,double> prediction, int direction)
        {
            double[] probs = PredictionToProbabilities(prediction);
            int offset = direction / 90;

            Array.Clear(_tempPdf,0,_tempPdf.Length);
            for(int index=0;index<_viewMap.Length;index+=4)
            {
                for (int a = 0; a < 4; a++)
                {
                    int labelIndex = _viewMap[index + ((a + offset) % 4)];
                    _tempPdf[index + a] += probs[labelIndex] / _labelCount[labelIndex];
                }
            }
        }
        public void OnMeasurementCompleted(int count)
        {
            MathHelper.Divide(_tempPdf, count);
            ProbabilityHelper.BayesTheorem(_belief, _tempPdf, MinProbability, _belief);
            LocalizationUpdated.Invoke(this, new LocalizationEventArgs()
            {
                Belief = _belief
            });
        }

        #endregion

        private double[] PredictionToProbabilities(Dictionary<string, double> prediction)
        {
            double[] probs = new double[_mapLabels.Length];
            foreach(var item in prediction)
            {
                int labelIndex = Array.IndexOf(_mapLabels, item.Key);
                /* Если нет в словаре статических объектов, значит,
                 * объект динамический и его точка - пустой квадрат */
                if (labelIndex == -1)
                    labelIndex = 0;
                probs[labelIndex] += item.Value;
            }
            double uniformProb = 1f / _mapLabels.Length;
            for (int i = 0; i < _mapLabels.Length; i++)
                probs[i] = probs[i] * MeasurementSuccessProb + (1 - MeasurementSuccessProb) * uniformProb;
            return probs;
        }
    }
}
