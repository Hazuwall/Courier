using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Courier
{
    public partial class ProbabilityWindow : System.Windows.Window
    {
        private const int ElementSize = 50;

        private readonly NavigationSystem _navigation;

        public ProbabilityWindow(NavigationSystem navigation)
        {
            InitializeComponent();
            _navigation = navigation;
            _navigation.ProbabilityChanged += Navigation_ProbabilityChanged;

            Brush brush = new SolidColorBrush(Colors.Blue);
            for (int x = 0; x < _navigation.WorldShape[0]; x++)
            {
                for (int y = _navigation.WorldShape[1]-1; y>=0; y--)
                {
                    for (int a = 0; a < 360; a += 90)
                    {
                        Polygon triangle = new Polygon()
                        {
                            Points = GetTrianglePoints(x, y, a),
                            Fill = brush,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };
                        ProbabilityCanvas.Children.Add(triangle);
                    }
                }
            }
            for (int x = 0; x < _navigation.WorldShape[0]; x++)
            {
                for (int y = _navigation.WorldShape[1] - 1; y >= 0; y--)
                {
                    Ellipse centerCircle = new Ellipse()
                    {
                        Fill = new SolidColorBrush(Colors.Azure),
                        Height = ElementSize / 5,
                        Width = ElementSize / 5
                    };
                    Canvas.SetLeft(centerCircle, x * ElementSize + 0.4 * ElementSize);
                    Canvas.SetBottom(centerCircle, y * ElementSize + 0.4 * ElementSize);
                    ProbabilityCanvas.Children.Add(centerCircle);
                }
            }
        }

        private static PointCollection GetTrianglePoints(int x, int y, int angle)
        {
            var points = new System.Windows.Point[3];
            points[0] = new System.Windows.Point((x + 0.5) * ElementSize, (y + 0.5) * ElementSize);
            switch (angle)
            {
                case 0:
                    points[1] = new System.Windows.Point((x+1) * ElementSize, y * ElementSize);
                    points[2] = new System.Windows.Point((x+1) * ElementSize, (y + 1) * ElementSize);
                    break;
                case 90:
                    points[1] = new System.Windows.Point(x* ElementSize, y * ElementSize);
                    points[2] = new System.Windows.Point((x + 1) * ElementSize, y * ElementSize);
                    break;
                case 180:
                    points[1] = new System.Windows.Point(x * ElementSize, y * ElementSize);
                    points[2] = new System.Windows.Point(x * ElementSize, (y + 1) * ElementSize);
                    break;
                default:
                    points[1] = new System.Windows.Point(x * ElementSize, (y + 1) * ElementSize);
                    points[2] = new System.Windows.Point((x+1) * ElementSize, (y + 1) * ElementSize);
                    break;
            }
            return new PointCollection(points);
        }

        private void Navigation_ProbabilityChanged(object sender, NavigationSystem.LocalizationEventArgs e)
        {
            int indexStep = MathHelper.GetOneDimensionalIndexFactors(_navigation.WorldShape)[1];
            var items = ProbabilityCanvas.Children;
            var belief = _navigation.Belief;
            int positionCount = belief.Length / indexStep;
            double max = belief.Max();
            for (int i = 0; i < positionCount; i++)
                for(int a=0;a<4;a++)
                    items[i*4 + a].Opacity = Math.Pow(belief[indexStep * i + a] / max, 0.1);

            double[,] tKernel = e.TranslationKernel;
            if (tKernel != null)
            {
                byte[] bytes = DrawingHelper.ArrayToImageData(tKernel);
                TranslationKernelImage.Source = DrawingHelper.ToImageSource(bytes,
                    tKernel.GetLength(1),
                    tKernel.GetLength(0));
            }
            double[] rKernel = e.RotationKernel;
            if (rKernel != null)
            {
                byte[] bytes = DrawingHelper.ArrayToImageData(rKernel);
                RotationKernelImage.Source = DrawingHelper.ToImageSource(bytes,
                    rKernel.Length, 1);
            }
        }
    }
}
