using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Courier
{
    public class WorldObject
    {
        public ModelBase Model { get; }
        public UIElement View { get; }

        public double Size { get; }

        private Point _point;
        public Point Point
        {
            get { return _point; }
            set
            {
                UpdateViewPosition(_point, value);
                _point = value;
            }
        }

        private int _orientation;
        public int Orientation {
            get { return _orientation; }
            set {
                int temp = MathHelper.GetOneTurn(value);
                UpdateViewOrientation(_orientation, value);
                _orientation = temp;
            }
        }

        public WorldObject(ModelBase model, double size, Point point, int orientation)
        {
            Model = model;
            View = CreateView(model.Class, size, point, orientation, model.DoCollide ? 1 : 0);

            Size = size;
            _point = point;
            _orientation = orientation;
        }
        public WorldObject(ModelBase model, double size, Point point) : this(model, size, point, -90)
        {
        }

        private UIElement CreateView(string className, double size, Point point, int orientation, int zIndex)
        {
            Uri imageUri = new Uri(String.Format("/Icons/{0}.png", className), UriKind.Relative);
            var view = new Image()
            {
                Width = size,
                Height = size,
                Source = new BitmapImage(imageUri),
                RenderTransform = new RotateTransform(-90-orientation, size/2, size/2)
            };
            Canvas.SetLeft(view, point.X * size + size / 2);
            Canvas.SetBottom(view, point.Y * size + size / 2);
            Canvas.SetZIndex(view, zIndex);
            return view;
        }

        private void UpdateViewPosition(Point p1, Point p2)
        {
            Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            DoubleAnimation animation = new DoubleAnimation(p1.X * Size + Size / 2,
                p2.X * Size + Size / 2, duration);
            View.BeginAnimation(Canvas.LeftProperty, animation);
            animation = new DoubleAnimation(p1.Y * Size + Size / 2,
                p2.Y * Size + Size / 2, duration);
            View.BeginAnimation(Canvas.BottomProperty, animation);
        }

        private void UpdateViewOrientation(int o1, int o2)
        {
            Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            DoubleAnimation animation = new DoubleAnimation(-90 - o1, -90 - o2, duration);
            View.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }
    }
}
