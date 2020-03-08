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
    public partial class LocalizationWindow : System.Windows.Window
    {
        public LocalizationWindow(SimulationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            double size = viewModel.CanvasSize / viewModel.WorldShape[0];
            AddLocalizationIndicators(viewModel.LocalizationIndicators, viewModel.WorldShape[0], viewModel.WorldShape[1], size);
        }

        private static void AddLocalizationIndicators(ICollection<UIElement> collection, int horizontalCoumt, int verticalCount, double size)
        {
            Brush brush = new SolidColorBrush(Colors.Blue);
            for (int x = 0; x < horizontalCoumt; x++)
            {
                for (int y = verticalCount - 1; y >= 0; y--)
                {
                    for (int a = 0; a < 360; a += 90)
                    {
                        Polygon triangle = new Polygon()
                        {
                            Points = GetTrianglePoints(x, y, a, size),
                            Fill = brush,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };
                        collection.Add(triangle);
                    }
                }
            }
            for (int x = 0; x < horizontalCoumt; x++)
            {
                for (int y = verticalCount - 1; y >= 0; y--)
                {
                    Ellipse centerCircle = new Ellipse()
                    {
                        Fill = new SolidColorBrush(Colors.Azure),
                        Height = size / 5,
                        Width = size / 5
                    };
                    Canvas.SetLeft(centerCircle, x * size + 0.4 * size);
                    Canvas.SetBottom(centerCircle, y * size + 0.4 * size);
                    collection.Add(centerCircle);
                }
            }
        }

        private static PointCollection GetTrianglePoints(int x, int y, int angle, double size)
        {
            var points = new System.Windows.Point[3];
            points[0] = new System.Windows.Point((x + 0.5) * size, (y + 0.5) * size);
            switch (angle)
            {
                case 0:
                    points[1] = new System.Windows.Point((x+1) * size, y * size);
                    points[2] = new System.Windows.Point((x+1) * size, (y + 1) * size);
                    break;
                case 90:
                    points[1] = new System.Windows.Point(x* size, y * size);
                    points[2] = new System.Windows.Point((x + 1) * size, y * size);
                    break;
                case 180:
                    points[1] = new System.Windows.Point(x * size, y * size);
                    points[2] = new System.Windows.Point(x * size, (y + 1) * size);
                    break;
                default:
                    points[1] = new System.Windows.Point(x * size, (y + 1) * size);
                    points[2] = new System.Windows.Point((x+1) * size, (y + 1) * size);
                    break;
            }
            return new PointCollection(points);
        }
    }
}
