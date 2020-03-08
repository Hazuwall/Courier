using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class Camera
    {
        private readonly World _world;

        public int Orientation { get; set; }
        public string[] KnownClasses { get; }
        public double[,] ConfusionMatrix { get; }

        public Camera(World world, string[] classes, double[,] confusionMatrix)
        {
            _world = world;
            KnownClasses = classes;
            ConfusionMatrix = confusionMatrix;
        }

        public Dictionary<string,double> Measure(ModelBase model, int orientation)
        {
            var obj = _world.Find(model);
            int absOrientation = MathHelper.GetOneTurn(obj.Orientation + orientation);
            Point point = obj.Point + new Point(absOrientation);
            
            string seenClass = _world.FindOrDefault(point)?.Model.Class;
            if (seenClass == null)
                seenClass = ModelBase.EmptyClassName;
            int trueClassIndex = Array.IndexOf(KnownClasses, seenClass);
            if (trueClassIndex == -1)
            {
                seenClass = ModelBase.UnknownClassName;
                trueClassIndex = Array.IndexOf(KnownClasses, seenClass);
            }

            var probs = new Dictionary<string, double>();
            for (int i = 0; i < KnownClasses.Length; i++)
                probs.Add(KnownClasses[i], ConfusionMatrix[trueClassIndex, i]);
            return probs;
        }
    }
}
