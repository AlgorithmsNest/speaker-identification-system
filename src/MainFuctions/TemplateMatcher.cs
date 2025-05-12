using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recorder
{
    public class TemplateMatcher
    {
        public string Name { get; }
        private MFCCFrame[] Template;
        private double[] prevRow;
        private double[] currRow;

        public TemplateMatcher(string name, MFCCFrame[] template)
        {
            Name = name;
            Template = template;
            prevRow = new double[template.Length + 1];
            currRow = new double[template.Length + 1];

            for (int j = 0; j <= template.Length; j++)
                prevRow[j] = double.PositiveInfinity;
            prevRow[0] = 0;
        }

        public void match(MFCCFrame inputFrame)
        {
            for (int j = 0; j <= Template.Length; j++)
                currRow[j] = double.PositiveInfinity;

            for (int j = 1; j <= Template.Length; j++)
            {
                double dist = DTW.EuclideanDistance(inputFrame, Template[j - 1]);

                double minPrev = Math.Min(prevRow[j], prevRow[j - 1]);
                if (j >= 2)
                    minPrev = Math.Min(minPrev, prevRow[j - 2]);

                currRow[j] = dist + minPrev;
            }

            var temp = prevRow;
            prevRow = currRow;
            currRow = temp;
        }

        public double CurrentScore => prevRow[Template.Length];
    }
}
