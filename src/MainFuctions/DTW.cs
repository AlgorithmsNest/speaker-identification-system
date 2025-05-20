using AForge.Math.Metrics;
using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Accord.Math;

namespace Recorder
{
    public static class DTW
    {
        internal static double EuclideanDistance(MFCCFrame a, MFCCFrame b)
        {
            /*if (a?.Features == null || b?.Features == null)
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
            }*/

            // 13 features for each frame
            double sum = 0;
            //Console.WriteLine("---------------------------------");
            for (int i = 0; i < a.Features.Length; i++)
            {
                //if (a.Features[i] == b.Features[i])
                //    Console.WriteLine(a.Features[i] + " : " + b.Features[i]);
                double diff = a.Features[i] - b.Features[i];
                sum += (diff * diff);
            }
            //Console.WriteLine("---------------------------------");
            return Math.Sqrt(sum);
        }


        public static double DynamicTimeWarping(MFCCFrame[] input, MFCCFrame[] template)
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

            return prev[templateLen];
        }
        public static (string Name, double Score) MatchingVoicesTimeSync(MFCCFrame[] input, Dictionary<string, List<MFCCFrame[]>> templates)
        {
            string bestName = null;
            double bestScore = double.PositiveInfinity;

            foreach (var user in templates)
            {
                string userName = user.Key;
                List<MFCCFrame[]> userTemplates = user.Value;

                foreach (var template in userTemplates)
                {
                    var matcher = new TemplateMatcher(userName, template);
                    foreach (var frame in input)
                    {
                        matcher.match(frame);
                    }

                    double score = matcher.CurrentScore;
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestName = userName;
                    }
                }
            }

            if (bestName == null)
                return (null, double.PositiveInfinity);

            return (bestName, bestScore);
        }

        // return type is pair but here named tuple , name of user of best mached voice with , min distance
        public static (string bestMatchName, double matchDistance) MatchingWithTemplatesDTW(MFCCFrame[] inputFrames, Dictionary<string, List<MFCCFrame[]>> templates)
        {

            //Console.WriteLine("HERE");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string bestMatch = null;
            double minDistance = double.PositiveInfinity;
            double distance;

            foreach (var kvp in templates)
            {
                //Console.WriteLine("I'm in loop count me---");
                string user = kvp.Key;
                foreach (var tempalate_voice in kvp.Value) // templates[user]
                {
                    MFCCFrame[] template = tempalate_voice;
                    distance = DynamicTimeWarping(inputFrames, template);
                    //Console.WriteLine("Distance in loop with each template frame: " + distance);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestMatch = user;
                    }
                }


            }
            //Console.WriteLine("Min Distance is: " + minDistance); // not Console.WriteLine("Min Distance is: " , minDistance);
            //Console.WriteLine("Normal DTW--- Elapsed Time in ms: " + stopwatch.ElapsedMilliseconds + " ms");
            //Console.WriteLine("Normal DTW--- Elapsed Time in sec: " + stopwatch.Elapsed.TotalSeconds + " s");
            return (bestMatch,minDistance);
            
        }

    }
    public static class Prunning
    {
        const double INF = double.PositiveInfinity;
        public static double PruningLimitngSearchPath(MFCCFrame[] input, MFCCFrame[] template, int Width)
        {

            //var DiagonalContainer = new Dictionary<(int, int), double>(); //DP [(i,j), DpVal]

            int InputFramesNO = input.Length; // rows 
            int TemplateFramesNo = template.Length; // cols
            if (InputFramesNO == 0 || TemplateFramesNo == 0)
                return INF;


            // NEW 
            var prev = new Dictionary<int, double>(); // represents column number in previous row
            var curr = new Dictionary<int, double>(); // represents column number in current row
            int HalfWidth = Width / 2;
            HalfWidth = Math.Max(HalfWidth, Math.Abs(InputFramesNO - TemplateFramesNo));


            for (int TemplateFrame = 0; TemplateFrame <= HalfWidth && TemplateFrame <= TemplateFramesNo; TemplateFrame++)
                prev[TemplateFrame] = INF;
            prev[0] = 0;

            for (int InputFrame = 1; InputFrame <= InputFramesNO; InputFrame++)
            {
                int StartColumn = Math.Max(1, InputFrame - HalfWidth);
                int EndColumn = Math.Min(TemplateFramesNo, InputFrame + HalfWidth);
                curr.Clear();
                for (int j = StartColumn; j <= EndColumn; j++)
                    curr[j] = double.PositiveInfinity;
                for (int TemplateFrame = StartColumn; TemplateFrame <= EndColumn; TemplateFrame++)
                {
                    double choice1 = INF; //corresponding
                    double choice2 = INF; //stretching
                    double choice3 = INF; //shrinking

                    if (prev.ContainsKey(TemplateFrame - 1))
                    {  //Avoiding out of matrix corresponding
                        choice1 = prev[TemplateFrame - 1];
                    }
                    if (prev.ContainsKey(TemplateFrame)) //Avoiding out of the diagonal region
                    {
                        choice2 = prev[TemplateFrame];
                    }
                    if (prev.ContainsKey(TemplateFrame - 2) && TemplateFrame != StartColumn) //Avoiding out of matrix corresponding and out of the diagonal region
                    {
                        choice3 = prev[TemplateFrame - 2];
                    }

                    if (choice1 == INF && choice2 == INF && choice3 == INF)
                    {
                        curr[TemplateFrame] = INF; // to avoid overflow cuz it will be INF + distance
                    }
                    else
                    {
                        curr[TemplateFrame] = Math.Min(choice1, Math.Min(choice2, choice3)) +
                            DTW.EuclideanDistance(input[InputFrame - 1], template[TemplateFrame - 1]);
                    }

                }

                /* prev = new Dictionary<int, double>();
                 // move curr to prev
                 foreach (var kvp in curr)
                 {
                     prev[kvp.Key] = kvp.Value;
                 }*/
                prev = new Dictionary<int, double>(curr);

            }
            // FOR TESTING
            //Console.WriteLine("Size of map(Number of elements) : " + DiagonalContainer.Count);
            //Console.WriteLine("DP[n][n]: " + DiagonalContainer[(InputFramesNO - 1, InputFramesNO - 1)]); // main diagonal i = j
            //Console.WriteLine("DP[n][EndCol]: " + DiagonalContainer[(InputFramesNO - 1, Math.Min(TemplateFramesNo - 1 , InputFramesNO - 1 + HalfWidth))]);
            /*foreach(var kvp in DiagonalContainer)
            {
                Console.WriteLine("(" + kvp.Key.Item1 + "," + kvp.Key.Item2 + ")" + " = " + kvp.Value);
            }*/
            // FOR TESTING
            if (prev.ContainsKey(TemplateFramesNo))
            {
                return prev[TemplateFramesNo];
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


        public static (string,double) PruningMatchingSearchPath(MFCCFrame[] inputFrames, Dictionary<string, List<MFCCFrame[]>> templates, int width)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string bestMatch = null;
            double minDistance = double.PositiveInfinity;
            double distance;

            foreach (var kvp in templates)
            {
                string user = kvp.Key;
                foreach (var tempalate_voice in kvp.Value) // templates[user]
                {
                    MFCCFrame[] template = tempalate_voice;
                    distance = PruningLimitngSearchPath(inputFrames, template, width);
                    //Console.WriteLine("Distance in loop with each template frame: " + distance);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestMatch = user;
                    }
                }

            }
            stopwatch.Stop();
            // comment it in Complete testing
            //Console.WriteLine("Pruning Search Path--- Elapsed Time in ms: " + stopwatch.ElapsedMilliseconds + " ms");
            //Console.WriteLine("Pruning Search Path--- Elapsed Time in sec: " + stopwatch.Elapsed.TotalSeconds + " s");
            return (bestMatch,minDistance);
        }

        public static (string,double) PruningMatchingPathCost(MFCCFrame[] inputFrames, Dictionary<string, List<MFCCFrame[]>> templates, int beam_width)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string bestMatch = null;
            double minDistance = double.PositiveInfinity;
            double distance;

            foreach (var kvp in templates)
            {
                string user = kvp.Key;
                foreach (var tempalate_voice in kvp.Value) // templates[user]
                {
                    MFCCFrame[] template = tempalate_voice;
                    distance = PruningLimitngPathCost(inputFrames, template, beam_width);
                    //Console.WriteLine("Distance in loop with each template frame: " + distance);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestMatch = user;
                    }
                }

            }
            stopwatch.Stop();
            Console.WriteLine("Pruning Path Cost--- Elapsed Time in ms: " + stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("Pruning Path Cost--- Elapsed Time in sec: " + stopwatch.Elapsed.TotalSeconds + " s");
            return (bestMatch, minDistance);
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
    }
    public class TemplateMatcher
    {
        public string Name { get; }
        private MFCCFrame[] Template;
        private double[] prevRow;
        private double[] currRow;

        public TemplateMatcher(string name, MFCCFrame[] template) // templates (voices) of a user
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

                double minPrev = prevRow[j];
                if (j >= 1) minPrev = Math.Min(minPrev, prevRow[j - 1]);
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
} ///////
