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
        public const int ObjectSize = 50;

        public ModelBase Model { get; }
        public UIElement View { get; }

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
                UpdateViewOrientation(_orientation, temp);
                _orientation = temp;
            }
        }

        public WorldObject(ModelBase model, Point point, int orientation)
        {
            Model = model;
            View = CreateView(model.Class, point, orientation);

            _point = point;
            _orientation = orientation;
        }
        public WorldObject(ModelBase model, Point point) : this(model, point, 0)
        {
        }

        private UIElement CreateView(string className, Point point, int orientation)
        {
            Uri imageUri = new Uri(String.Format("/Icons/{0}.png", className), UriKind.Relative);
            var view = new Image()
            {
                Width = ObjectSize,
                Height = ObjectSize,
                Source = new BitmapImage(imageUri),
                RenderTransform = new RotateTransform(270 - orientation, ObjectSize/2, ObjectSize/2)
            };
            Canvas.SetLeft(view, point.X * ObjectSize + ObjectSize / 2);
            Canvas.SetBottom(view, point.Y * ObjectSize + ObjectSize / 2);
            return view;
        }

        private void UpdateViewPosition(Point p1, Point p2)
        {
            Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            DoubleAnimation animation = new DoubleAnimation(p1.X * ObjectSize + ObjectSize / 2,
                p2.X * ObjectSize + ObjectSize / 2, duration);
            View.BeginAnimation(Canvas.LeftProperty, animation);
            animation = new DoubleAnimation(p1.Y * ObjectSize + ObjectSize / 2,
                p2.Y * ObjectSize + ObjectSize / 2, duration);
            View.BeginAnimation(Canvas.BottomProperty, animation);
        }

        private void UpdateViewOrientation(int o1, int o2)
        {
            Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            DoubleAnimation animation = new DoubleAnimation(270 - o1, 270 - o2, duration);
            View.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }
    }
}
