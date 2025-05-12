using AForge.Math.Metrics;
using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recorder
{
    public static class DTW
    {
        public static double EuclideanDistance(MFCCFrame a, MFCCFrame b)
        {
            double sum = 0;
            for (int i = 0; i < 13; i++)
            {
                double diff = a.Features[i] - b.Features[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        public static double MatchingVoices(MFCCFrame[] input, MFCCFrame[] template)
        {

            double[] prev = new double[template.Length + 1];
            double[] curr = new double[template.Length + 1];

            for (int j = 0; j <= template.Length; j++)
                prev[j] = double.PositiveInfinity;
            prev[0] = 0;

            for (int i = 1; i <= input.Length; i++)
            {
                for (int j = 0; j <= template.Length; j++)
                    curr[j] = double.PositiveInfinity;

                for (int j = 1; j <= template.Length; j++)
                {
                    double dist = EuclideanDistance(input[i - 1], template[j - 1]);

                    double minPrev = Math.Min(prev[j], prev[j - 1]);
                    if (j >= 2)
                        minPrev = Math.Min(minPrev, prev[j - 2]);

                    curr[j] = dist + minPrev;
                }
                double[] temp = prev;
                prev = curr;
                curr = temp;
            }

            return prev[template.Length];

        }
        public static string MatchingVoicesTimeSync(MFCCFrame[] input, Dictionary<string, MFCCFrame[]> templates)
        {
            var matchers = templates
                .Select(t => new TemplateMatcher(t.Key, t.Value))
                .ToList();

            foreach (var frame in input)
            {
                Parallel.ForEach(matchers, matcher =>
                {
                    matcher.match(frame);
                });
            }

            var best = matchers.OrderBy(m => m.CurrentScore).FirstOrDefault();
            return best?.Name;
        }
    }
}