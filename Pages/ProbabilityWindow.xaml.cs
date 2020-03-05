using System;
using System.Collections.Generic;
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
            for (int x = 0; x < _navigation.Size[0]; x++)
            {
                for (int y = _navigation.Size[1]-1; y>=0; y--)
                {
                    for (int a = 0; a < 360; a += 90)
                    {
                        Polygon triangle = new Polygon()
                        {
                            Points = GetTrianglePoints(x, y, a),
                            Fill = brush,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };
                        //Canvas.SetLeft(triangle, x * ElementSize + ElementSize / 2);
                        //Canvas.SetBottom(triangle, y * ElementSize + ElementSize / 2);
                        ProbabilityCanvas.Children.Add(triangle);
                    }
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

        private void Navigation_ProbabilityChanged(object sender, EventArgs e)
        {
            int indexStep = MathHelper.GetIndexFactors(_navigation.Size)[1];
            double max = _navigation.Belief.Max();
            var items = ProbabilityCanvas.Children;
            int positionCount = items.Count / 4;
            for (int i = 0; i < positionCount; i++)
                for(int a=0;a<4;a++)
                    items[i*4 + a].Opacity = Math.Pow(_navigation.Belief[indexStep * i + a] / max, 0.1);
        }
    }
}
