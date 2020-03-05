using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class NavigationSystem
    {
        private struct State
        {
            public int X;
            public int Y;
            public int Z;
            public int A;
        }

        private readonly ILookup<string, State> _map;
        private readonly double[] _tempProbs;
        
        public int[] Size { get; }
        public double[] Belief { get; }
        public event EventHandler ProbabilityChanged = delegate { };

        public NavigationSystem(World world)
        {
            _map = CreateMap(world);
            Point end = world.UpperBound;
            Size = new int[] { end.X + 1, end.Y + 1, end.Z + 1, 4 };
            int length = Size[0] * Size[1] * Size[2] * Size[3];
            Belief = new double[length];
            _tempProbs = new double[length];
            double uniformProb = 1f / length;
            for (int i = 0; i < Belief.Length; i++)
                Belief[i] = uniformProb;
        }

        private static ILookup<string, State> CreateMap(World world)
        {
            Dictionary<string, List<State>> tempMap = new Dictionary<string, List<State>>();

            Point lower = world.LowerBound;
            Point upper = world.UpperBound;
            bool[,,] filledMask = new bool[upper.X+1,upper.Y+1,upper.Z+1];
            List<State> filledList = new List<State>();

            foreach (var obj in world.Objects)
            {
                ModelBase model = obj.Model;
                if (model.IsStatic && model.DoCollide)
                {
                    Point p = obj.Point;
                    filledList.Add(new State()
                    {
                        X = p.X,
                        Y = p.Y,
                        Z = p.Z
                    });
                    filledMask[p.X, p.Y, p.Z] = true;

                    string className = model.Class;
                    List<State> groupList;
                    if (!tempMap.TryGetValue(className, out groupList))
                    {
                        groupList = new List<State>();
                        tempMap.Add(className, groupList);
                    }
                    AddSurroundingToList(groupList, p.X, p.Y, p.Z);
                }
            }

            List<State> emptyList = new List<State>();
            for (int x = 0; x < filledMask.GetLength(0); x++)
                for (int y = 0; y < filledMask.GetLength(1); y++)
                    for (int z = 0; z < filledMask.GetLength(2); z++)
                        if (!filledMask[x, y, z])
                            AddSurroundingToList(emptyList, x, y, z);    
            tempMap.Add(ModelBase.EmptyClassName, emptyList);

            return tempMap
                .SelectMany(group => group.Value
                    .Except(filledList)
                    .Where(loc => (loc.X >= lower.X) && (loc.X <= upper.X) && (loc.Y >= lower.Y) && (loc.Y <= upper.Y))
                    .Select(loc => new KeyValuePair<string, State>(group.Key, loc)))
                .ToLookup(pair => pair.Key, pair => pair.Value);
        }

        private static void AddSurroundingToList(List<State> group, int x, int y, int z)
        {
            group.Add(new State
            {
                X = x + 1,
                Y = y,
                Z = z,
                A = 2
            });
            group.Add(new State
            {
                X = x - 1,
                Y = y,
                Z = z,
                A = 0
            });
            group.Add(new State
            {
                X = x,
                Y = y + 1,
                Z = z,
                A = 3
            });
            group.Add(new State
            {
                X = x,
                Y = y - 1,
                Z = z,
                A = 1
            });
        }

        public static void RecalculateProbs(double[] prior, double[] posterior, int[]size)
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

        public void OnMovement(int direction, int distance)
        {
            
                        
        }

        public void OnRotation(int angle)
        {
            Array.Clear(_tempProbs, 0, _tempProbs.Length);
            int offset = MathHelper.GetOneTurn(angle) / 90;
            int indexStep = MathHelper.GetIndexFactors(Size)[2];
            double[] temp = new double[4];
            for (int i = 0; i < _tempProbs.Length; i += indexStep)
            {
                for (int a = 0; a < 4; a++)
                    temp[a] = _tempProbs[i + a];
                for (int a = 0; a < 4; a++)
                    _tempProbs[i + (a + offset) % 4] = temp[a];
            }
            ProbabilityChanged.Invoke(this, new EventArgs());
        }

        public void OnMeasurement(Dictionary<string,double> probs)
        {
            Array.Clear(_tempProbs,0,_tempProbs.Length);
            int[] factors = MathHelper.GetIndexFactors(Size);
            foreach (var classProb in probs)
            {
                var locations = _map[classProb.Key];
                int count = locations.Count();
                foreach (var loc in locations)
                {
                    _tempProbs[loc.X * factors[0] + loc.Y * factors[1] + loc.Z * factors[2] + loc.A] += classProb.Value / count;
                }
            }
            RecalculateProbs(Belief, _tempProbs, Size);
            ProbabilityChanged.Invoke(this, new EventArgs());
        }
    }
}
