using AForge.Math.Metrics;
using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
<<<<<<< HEAD
=======
using System.Diagnostics;
>>>>>>> 29c4b1c8c70fbe4a195fdf5cc8bd0931abdc4f5b

namespace Recorder
{
    public static class DTW
    {
        public static double EuclideanDistance(MFCCFrame a, MFCCFrame b)
        {
            if (a?.Features == null || b?.Features == null)
            {
                //Console.WriteLine("Warning: One or both MFCCFrames have null Features.");
                return double.PositiveInfinity;
            }

            if (a.Features.Length != b.Features.Length)
            {
               // Console.WriteLine("Warning: Feature length mismatch.");
                return double.PositiveInfinity;
            }

            bool hasInvalidA = a.Features.Any(f => double.IsNaN(f) || double.IsInfinity(f));
            bool hasInvalidB = b.Features.Any(f => double.IsNaN(f) || double.IsInfinity(f));

            if (hasInvalidA || hasInvalidB)
            {              
                return double.PositiveInfinity; // Return Infinity for invalid frames
            }

            double sum = 0;
            for (int i = 0; i < a.Features.Length; i++)
            {
                double diff = a.Features[i] - b.Features[i];
                sum += diff * diff;
            }

            return Math.Sqrt(sum);
        }


<<<<<<< HEAD
        public static double MatchingVoices(MFCCFrame[] input, MFCCFrame[] template)
=======
        public static double DynamicTimeWarping(MFCCFrame[] input, MFCCFrame[] template)
>>>>>>> 29c4b1c8c70fbe4a195fdf5cc8bd0931abdc4f5b
        {
            if (input.Length == 0 || template.Length == 0)
                return double.PositiveInfinity;

            int inputLen = input.Length;
            int templateLen = template.Length;

            double[] prev = new double[templateLen + 1];
            double[] curr = new double[templateLen + 1];

            for (int j = 0; j <= templateLen; j++)
                prev[j] = double.PositiveInfinity;
            prev[0] = 0;

            for (int i = 1; i <= inputLen; i++)
            {
                for (int j = 0; j <= templateLen; j++)
                    curr[j] = double.PositiveInfinity;

                for (int j = 1; j <= templateLen; j++)
                {
                    double dist = EuclideanDistance(input[i - 1], template[j - 1]);
                    double minPrev = prev[j];

                    if (j >= 1) minPrev = Math.Min(minPrev, prev[j - 1]);
                    if (j >= 2) minPrev = Math.Min(minPrev, prev[j - 2]);

                    curr[j] = dist + minPrev;
                }

                var temp = prev;
                prev = curr;
                curr = temp;
            }

            return prev[templateLen] / Math.Max(inputLen, templateLen);
        }

<<<<<<< HEAD
=======

        public static string MatchingWithTemplatesDTW(MFCCFrame[] inputFrames, Dictionary<string, MFCCFrame[]> templates)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string bestMatch = null;
            double minDistance = double.PositiveInfinity;
            double distance;

            foreach (var kvp in templates)
            {
                string user = kvp.Key;
                MFCCFrame[] template = kvp.Value;
                distance = DynamicTimeWarping(inputFrames, template);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = user;
                }

            }
            Console.WriteLine("Normal DTW--- Elapsed Time in ms: " + stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("Normal DTW--- Elapsed Time in sec: " + stopwatch.Elapsed.TotalSeconds + " s");
            return bestMatch;
        }

>>>>>>> 29c4b1c8c70fbe4a195fdf5cc8bd0931abdc4f5b
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
            if (DiagonalContainer.ContainsKey((InputFramesNO - 1, TemplateFramesNo - 1)))
            {
                return DiagonalContainer[(InputFramesNO - 1, TemplateFramesNo - 1)];
            }
            return INF;
        }

        public static double PruningLimitngPathCost(MFCCFrame[] input, MFCCFrame[] template, int BeamWidth)
        {
            int InputFramesNO = input.Length;
            int TemplateFramesNo = template.Length;
            var Prev = new Dictionary<int, double>(); // columns(template frames) of previous row (input frames) >> cost
            var Curr = new Dictionary<int, double>(); // columns(template frames) of current row (input frames) >> cost
            var NextTemplateFrames = new HashSet<int>(); // valid columns(template frames) of next row (input frames) that we need to calculate its cost

            double BestCost = INF;
            for (int TempalteFrame = 0; TempalteFrame < TemplateFramesNo; TempalteFrame++)
            {
                Curr[TempalteFrame] = DTW.EuclideanDistance(input[0], template[TempalteFrame]);
                BestCost = Math.Min(BestCost, Prev[TempalteFrame]);
            }
            double Threshold = BestCost + BeamWidth;
            for (int TempalteFrame = 0; TempalteFrame < TemplateFramesNo; TempalteFrame++)
            {
                if (Curr[TempalteFrame] <= Threshold)
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
                    // we could use prev and delete if not , but I think it will take more complexity idk
                    Prev.Add(TempalteFrame, Curr[TempalteFrame]); // only add which are less than or equal to threshold
                }

            }

            for (int InputFrame = 1; InputFrame < InputFramesNO; InputFrame++)
            {
                Curr = new Dictionary<int, double>();
                BestCost = INF;
                foreach (var col in NextTemplateFrames)  //which templates i want to calc this time, (iterate through the columns that I'm sure they will have path from prev row)
                {
                    double choice1 = INF; //corresponding
                    double choice2 = INF; //stretching
                    double choice3 = INF; //shrinking
                    // col represent column in current row
                    // prev represent columns in previous row
                    // curr represent columns in current row

                    // check transitions is valid from previous row , they exist in map or not, if not so their value was infinity (exceeds threshold)
                    // we don't need to check it goes out of boundary of array because it's map so it will not matter
                    if (Prev.ContainsKey(col - 1))
                    {
                        choice1 = Prev[col - 1]; // Diagonal (Corresponding)
                    }
                    if (Prev.ContainsKey(col))
                    {
                        choice2 = Prev[col]; // Stretching
                    }
                    if (Prev.ContainsKey(col - 2))
                    {
                        choice3 = Prev[col - 2]; // Shrinking
                    }
                    Curr[col] = Math.Min(choice1, Math.Min(choice2, choice3)) +
                        DTW.EuclideanDistance(input[InputFrame], template[col]);
                    BestCost = Math.Min(BestCost, Curr[col]);

                }
                Prev = new Dictionary<int, double>();
                NextTemplateFrames = new HashSet<int>(); //clear to reuse

                Threshold = BestCost + BeamWidth;
                foreach (var col in Curr)  //Threshold
                {
                    if (col.Value <= Threshold) // key index of column (Template frame) , Value is : Cumulative Cost (dp)
                    {
                        int move2 = col.Key + 1;
                        int move3 = col.Key + 2;
                        NextTemplateFrames.Add(col.Key); //indx of column
                        if (move2 < TemplateFramesNo)
                        {
                            NextTemplateFrames.Add(move2);
                        }
                        if (move3 < TemplateFramesNo)
                        {
                            NextTemplateFrames.Add(move3);
                        }
                        Prev.Add(col.Key, col.Value); // only add which are less than or equal to threshold
                    }

                }
            }
            if (Prev.ContainsKey(TemplateFramesNo - 1))
            {
                return Prev[TemplateFramesNo - 1];
            }
            else
                return INF;

        }

<<<<<<< HEAD
=======

        public static string PruningMatchingSearchPath(MFCCFrame[] inputFrames, Dictionary<string, MFCCFrame[]> templates,int width)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string bestMatch = null;
            double minDistance = double.PositiveInfinity;
            double distance;

            foreach (var kvp in templates)
            {
                string user = kvp.Key;
                MFCCFrame[] template = kvp.Value;
                distance = PruningLimitngPathCost(inputFrames, template,width);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = user;
                }

            }
            stopwatch.Stop();
            Console.WriteLine("Pruning Search Path--- Elapsed Time in ms: " + stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("Pruning Search Path--- Elapsed Time in sec: " + stopwatch.Elapsed.TotalSeconds + " s");
            return bestMatch;
        }

        public static string PruningMatchingPathCost(MFCCFrame[] inputFrames, Dictionary<string, MFCCFrame[]> templates,int beam_width)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string bestMatch = null;
            double minDistance = double.PositiveInfinity;
            double distance;

            foreach (var kvp in templates)
            {
                string user = kvp.Key;
                MFCCFrame[] template = kvp.Value;
                distance = PruningLimitngPathCost(inputFrames, template, beam_width);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = user;
                }

            }
            stopwatch.Stop();
            Console.WriteLine("Pruning Path Cost--- Elapsed Time in ms: " + stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("Pruning Path Cost--- Elapsed Time in sec: " + stopwatch.Elapsed.TotalSeconds + " s");
            return bestMatch;
        }

        public static bool checkValidWidthSearchPath(MFCCFrame[] inputFrames, MFCCFrame[] template)
        {
            // soon 
            return true;
        }

        public static bool checkValidWidthPathCost(MFCCFrame[] inputFrames, MFCCFrame[] template)
        {
            // soon 
            return true;
        }
>>>>>>> 29c4b1c8c70fbe4a195fdf5cc8bd0931abdc4f5b
    }
} ///////