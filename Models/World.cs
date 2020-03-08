using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class World
    {
        public Collection<WorldObject> Objects { get; }
        public Point LowerBound { get; }
        public Point UpperBound { get; }

        public World(Point lowerBound, Point upperBound, Collection<WorldObject> objects)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Objects = objects;
        }

        public WorldObject Find(ModelBase model)
        {
            return Objects.Single(t => t.Model == model);
        }
        public WorldObject FindOrDefault(Point point, bool doCollide = true)
        {
            return Objects.SingleOrDefault(t => t.Point == point && t.Model.DoCollide == doCollide);
        }
        public WorldObject FindNearestOrDefault(Point point)
        {
            return Objects.Where(t=>t.Model.DoCollide==true).OrderBy(t => t.Point.L1(point)).Skip(1).FirstOrDefault();
        }

        public World GetArea(Point lower, Point upper)
        {
            if(!(lower <= upper))
            {
                Point temp = lower;
                lower = upper;
                upper = temp;
            }
            if (!(upper <= UpperBound))
                upper = UpperBound;
            if (!(LowerBound <= lower))
                lower = LowerBound;

            Collection<WorldObject> objects = new Collection<WorldObject>();
            foreach (var obj in Objects.Where(t => t.Point.IsInside(lower, upper)))
                objects.Add(obj);
            return new World(lower, upper, objects);
        }
        
        public World GetArea(Point center, int distance)
        {
            if (distance < 0)
                throw new ArgumentException();
            Point lower = new Point(Math.Max(center.X - distance, LowerBound.X), Math.Max(center.Y - distance, LowerBound.Y), center.Z);
            Point upper = new Point(Math.Min(center.X + distance, UpperBound.X), Math.Max(center.Y - distance, UpperBound.Y), center.Z);
            Collection<WorldObject> objects = new Collection<WorldObject>();
            foreach (var obj in Objects.Where(t => t.Point.IsInside(lower, upper)))
                objects.Add(obj);
            return new World(lower, upper, objects);
        }

        public IEnumerable<WorldObject> Nearby(Point point)
        {
            return Objects.Where(t => t.Point.L1(point) == 1);
        }

        public bool IsFree(Point point)
        {
            return !Objects.Any(t => t.Point == point && t.Model.DoCollide == true);
        }

        public bool IsInside(Point point)
        {
            return point.IsInside(LowerBound, UpperBound);
        }


        public bool IsNearby(Point point, string className)
        {
            return Objects.Any(t => t.Point.L1(point) == 1 && t.Model.Class == className);
        }

        public int GetWetness(Point point)
        {
            var obj = Objects.FirstOrDefault(t => t.Point == point && t.Model.Class == WetFloor.ClassName);
            return obj == null ? 0 : (obj.Model as WetFloor).Wetness;
        }
    }
}
