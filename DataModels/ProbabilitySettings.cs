using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier
{
    public class ProbabilitySettings
    {
        public double DecideElevateProb { get; set; }
        public double DecideTranslateProb { get; set; }
        public double DecideRotateProb { get; set; }

        public double[,] ConfusionMatrix { get; set; }
        public double CameraSuccessStd { get; set; }

        public double ElevationSuccessProb { get; set; }
        public double TranslationStd { get; set; }
        public double TranslationWetnessFactor { get; set; }
        public double TranslationDistanceFactor { get; set; }
        public double RotationStd { get; set; }
        public double RotationWetnessFactor { get; set; }
        public double RotationAngleFactor { get; set; }

        public double EstimatedElevationStd { get; set; }
        public double EstimatedTranslationStd { get; set; }
        public double EstimatedRotationStd { get; set; }
        public double EstimatedCameraSuccessProb { get; set; }

        public static ProbabilitySettings ParseFromJson(string path)
        {
            string rawJson = File.ReadAllText(path);
            return (ProbabilitySettings)JsonConvert.DeserializeObject(rawJson, typeof(ProbabilitySettings));
        }
    }
}
