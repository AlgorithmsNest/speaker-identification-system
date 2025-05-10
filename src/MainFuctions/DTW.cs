using AForge.Math.Metrics;
using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }

    public static class Prunning
    {
        const double INF = double.PositiveInfinity;
        public static double PruningLimitngSearchPath(MFCCFrame[] input, MFCCFrame[] template, int Width)
        {

            var DiagonalContainer = new Dictionary<(int, int), double>(); //DP [(i,j), DpVal]
            if (Width % 2 == 0)
            {
                Width++;
            }
            int HalfWidth = Width / 2;
            for (int TempalteFrame = 0; TempalteFrame <= HalfWidth; TempalteFrame++)
            {
                DiagonalContainer[(0, TempalteFrame)] = DTW.EuclideanDistance(input[0], template[TempalteFrame]);
            }
            int InputFramesNO = input.Length;
            int TemplateFramesNo = template.Length;
            for (int InputFrame = 1; InputFrame < InputFramesNO; InputFrame++)
            {
                int StartColumn = Math.Max(0, InputFrame - HalfWidth);
                int EndColumn = Math.Min(TemplateFramesNo - 1, InputFrame + HalfWidth);
                for (int TemplateFrame = StartColumn; TemplateFrame <= EndColumn; TemplateFrame++)
                {
                    double choice1 = INF; //corresponding
                    double choice2 = INF; //stretching
                    double choice3 = INF; //shrinking

                    if (TemplateFrame - 1 >= 0)
                    {  //Avoiding out of matrix corresponding
                        choice1 = DiagonalContainer[(InputFrame - 1, TemplateFrame - 1)];
                    }
                    if (TemplateFrame != EndColumn) //Avoiding out of the diagonal region
                    {
                        choice2 = DiagonalContainer[(InputFrame - 1, TemplateFrame)];
                    }
                    if (TemplateFrame - 2 >= 0 && TemplateFrame != StartColumn) //Avoiding out of matrix corresponding and out of the diagonal region
                    {
                        choice3 = DiagonalContainer[(InputFrame - 1, TemplateFrame - 2)];
                    }

                    DiagonalContainer[(InputFrame, TemplateFrame)] = Math.Min(choice1, Math.Min(choice2, choice3)) +
                        DTW.EuclideanDistance(input[InputFrame], template[TemplateFrame]);
                }

            }
            int StartColumnForLast = (InputFramesNO - 1) - HalfWidth;
            int EndColumnForLast = TemplateFramesNo - 1;
            double MinimumMatchScore = INF;
            for (int LastInputFrameColumn = StartColumnForLast; LastInputFrameColumn <= EndColumnForLast; LastInputFrameColumn++)
            {
                MinimumMatchScore = Math.Min(MinimumMatchScore, DiagonalContainer[(InputFramesNO - 1, LastInputFrameColumn)]);
            }
            return MinimumMatchScore;
        }

        public static double PruningLimitngPathCost(MFCCFrame[] input, MFCCFrame[] template, int BeamWidth)
        {
            int InputFramesNO = input.Length;
            int TemplateFramesNo = template.Length;
            var Prev = new Dictionary<int, double>();
            var Curr = new Dictionary<int, double>();
            var NextTemplateFrames = new HashSet<int>();

            double BestCost = INF;
            for (int TempalteFrame = 0; TempalteFrame < TemplateFramesNo; TempalteFrame++)
            {
                Prev[TempalteFrame] = DTW.EuclideanDistance(input[0], template[TempalteFrame]);
                BestCost = Math.Min(BestCost, Prev[TempalteFrame]);
            }
            double Threshold = BestCost + BeamWidth;
            for (int TempalteFrame = 0; TempalteFrame < TemplateFramesNo; TempalteFrame++)
            {
                if (Prev[TempalteFrame] <= Threshold)
                {
                    int move2 = TempalteFrame + 1;
                    int move3 = TempalteFrame + 2;
                    NextTemplateFrames.Add(TempalteFrame); //i
                    if (move2 < TemplateFramesNo)
                    {
                        NextTemplateFrames.Add(move2);
                    }
                    if (move3 < TemplateFramesNo)
                    {
                        NextTemplateFrames.Add(move3);
                    }
                }
            }

            for (int InputFrame = 1; InputFrame < InputFramesNO; InputFrame++)
            {
                BestCost = INF;
                foreach (var item in NextTemplateFrames)  //which templates i want to calc this time
                {
                    double choice1 = INF; //corresponding
                    double choice2 = INF; //stretching
                    double choice3 = INF; //shrinking
                    if (Prev.ContainsKey(item - 1))
                    {
                        choice1 = Prev[item - 1];
                    }
                    if (Prev.ContainsKey(item))
                    {
                        choice2 = Prev[item];
                    }
                    if (Prev.ContainsKey(item - 2))
                    {
                        choice3 = Prev[item - 2];
                    }
                    Curr[item] = Math.Min(choice1, Math.Min(choice2, choice3)) +
                        DTW.EuclideanDistance(input[InputFrame], template[item]);
                    BestCost = Math.Min(BestCost, Prev[item]);

                }

                NextTemplateFrames = new HashSet<int>(); //clear to reuse

                Threshold = BestCost + BeamWidth;
                foreach (var item in NextTemplateFrames)  //Threshold
                {
                    int move2 = item + 1;
                    int move3 = item + 2;
                    NextTemplateFrames.Add(item); //i
                    if (move2 < TemplateFramesNo)
                    {
                        NextTemplateFrames.Add(move2);
                    }
                    if (move3 < TemplateFramesNo)
                    {
                        NextTemplateFrames.Add(move3);
                    }
                }
                Prev = Curr;          //is it ok????????
                Curr = new Dictionary<int, double>();     ///
            }
            double MinimumMatchScore = INF;
            for (int LastInputFrameColumn = 0; LastInputFrameColumn < TemplateFramesNo; LastInputFrameColumn++)
            {
                if (Prev.ContainsKey(LastInputFrameColumn))
                {
                    MinimumMatchScore = Math.Min(MinimumMatchScore, Prev[LastInputFrameColumn]);
                }
            }
            return MinimumMatchScore;
        }
    }
} ///////
