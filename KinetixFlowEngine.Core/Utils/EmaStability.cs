using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace KinetixFlowEngine.Core.Utils
{
    public class EmaStability
    {
        //private int _level1Mins = 15 * 12;
        //private int _level2Mins = 45 * 12;
        //private int _level3Mins = 135 * 12;

        private int minTicks = 12;

        public EmaStabilityState Compute(RollingWindowBuffer _scoreFast, RollingWindowBuffer _scoreMedium,
                            RollingWindowBuffer _scoreSlow, RollingWindowBuffer _probFast, RollingWindowBuffer _probMedium, RollingWindowBuffer _probSlow, double atr, double er)
        {
            double regime = Math.Clamp(atr * 0.65 + er * 0.35, 0.0, 1.0);
            int _level1Mins = (int)Lerp(10 * minTicks, 15 * minTicks, regime);
            int _level2Mins = (int)Lerp(30 * minTicks, 45 * minTicks, regime);
            int _level3Mins = (int)Lerp(90 * minTicks, 135 * minTicks, regime);

            var state = new EmaStabilityState();
            state.Level1 = _level1Mins;
            state.Level2 = _level2Mins;
            state.Level3 = _level3Mins;
            state.ScoreFastEmaLevel1 = Compute(_scoreFast, _level1Mins);
            state.ScoreMediumEmaLevel1 = Compute(_scoreMedium, _level1Mins);
            state.ScoreSlowEmaLevel1 = Compute(_scoreSlow, _level1Mins);
            state.ScoreFastEmaLevel2 = Compute(_scoreFast, _level2Mins);
            state.ScoreMediumEmaLevel2 = Compute(_scoreMedium, _level2Mins);
            state.ScoreSlowEmaLevel2 = Compute(_scoreSlow, _level2Mins);
            state.ScoreFastEmaLevel3 = Compute(_scoreFast, _level3Mins);
            state.ScoreMediumEmaLevel3 = Compute(_scoreMedium, _level3Mins);
            state.ScoreSlowEmaLevel3 = Compute(_scoreSlow, _level3Mins);

            state.ProbFastEmaLevel1 = Compute(_probFast, _level1Mins);
            state.ProbMediumEmaLevel1 = Compute(_probMedium, _level1Mins);
            state.ProbSlowEmaLevel1 = Compute(_probSlow, _level1Mins);
            state.ProbFastEmaLevel2 = Compute(_probFast, _level2Mins);
            state.ProbMediumEmaLevel2 = Compute(_probMedium, _level2Mins);
            state.ProbSlowEmaLevel2 = Compute(_probSlow, _level2Mins);
            state.ProbFastEmaLevel3 = Compute(_probFast, _level3Mins);
            state.ProbMediumEmaLevel3 = Compute(_probMedium, _level3Mins);
            state.ProbSlowEmaLevel3 = Compute(_probSlow, _level3Mins);

            state.FastScoreTrend = state.ScoreFastEmaLevel1 > state.ScoreFastEmaLevel2 && state.ScoreFastEmaLevel2 > state.ScoreFastEmaLevel3 ? StabilityDirection.Long
                                : state.ScoreFastEmaLevel1 < state.ScoreFastEmaLevel2 && state.ScoreFastEmaLevel2 < state.ScoreFastEmaLevel3 ? StabilityDirection.Short
                                : StabilityDirection.Neutral;
            state.MediumScoreTrend = state.ScoreMediumEmaLevel1 > state.ScoreMediumEmaLevel2 && state.ScoreMediumEmaLevel2 > state.ScoreMediumEmaLevel3 ? StabilityDirection.Long
                                : state.ScoreMediumEmaLevel1 < state.ScoreMediumEmaLevel2 && state.ScoreMediumEmaLevel2 < state.ScoreMediumEmaLevel3 ? StabilityDirection.Short
                                : StabilityDirection.Neutral;
            state.SlowScoreTrend = state.ScoreSlowEmaLevel1 > state.ScoreSlowEmaLevel2 && state.ScoreSlowEmaLevel2 > state.ScoreSlowEmaLevel3 ? StabilityDirection.Long
                                : state.ScoreSlowEmaLevel1 < state.ScoreSlowEmaLevel2 && state.ScoreSlowEmaLevel2 < state.ScoreSlowEmaLevel3 ? StabilityDirection.Short
                                : StabilityDirection.Neutral;
            state.FastProbTrend = state.ProbFastEmaLevel1 > state.ProbFastEmaLevel2 && state.ProbFastEmaLevel2 > state.ProbFastEmaLevel3 ? StabilityDirection.Long
                                : state.ProbFastEmaLevel1 < state.ProbFastEmaLevel2 && state.ProbFastEmaLevel2 < state.ProbFastEmaLevel3 ? StabilityDirection.Short
                                : StabilityDirection.Neutral;
            state.MediumProbTrend = state.ProbMediumEmaLevel1 > state.ProbMediumEmaLevel2 && state.ProbMediumEmaLevel2 > state.ProbMediumEmaLevel3 ? StabilityDirection.Long
                                : state.ProbMediumEmaLevel1 < state.ProbMediumEmaLevel2 && state.ProbMediumEmaLevel2 < state.ProbMediumEmaLevel3 ? StabilityDirection.Short
                                : StabilityDirection.Neutral;
            state.SlowProbTrend = state.ProbSlowEmaLevel1 > state.ProbSlowEmaLevel2 && state.ProbSlowEmaLevel2 > state.ProbSlowEmaLevel3 ? StabilityDirection.Long
                                : state.ProbSlowEmaLevel1 < state.ProbSlowEmaLevel2 && state.ProbSlowEmaLevel2 < state.ProbSlowEmaLevel3 ? StabilityDirection.Short
                                : StabilityDirection.Neutral;
            return state;
        }

        public static double Lerp(double min, double max, double t)
        {
            return min + (max - min) * Math.Clamp(t, 0.0, 1.0);
        }

        public decimal Compute(RollingWindowBuffer buffer, int levelMins)
        {
            var values = buffer.GetValues();
            if (values.Count == 0)
                return 0;

            var slice = values.TakeLast(levelMins).ToList();

            if (slice.Count < levelMins * 0.5) // require at least 50%
                return 0;

            return WeightedAverage(slice);
        }

        public decimal WeightedAverage(List<double> values)
        {
            int n = values.Count;
            if (n == 0) return 0;

            double sum = 0;
            double weightSum = 0;

            for (int i = 0; i < n; i++)
            {
                double weight = (i + 1) * (1 + 0.008 * i);
                sum += values[i] * weight;
                weightSum += weight;
            }

            return (decimal)(sum / weightSum);
        }
    }

    public enum StabilityDirection
    {
        Long,
        Short,
        Neutral
    }

    public class EmaStabilityState
    {
        public decimal ScoreFastEmaLevel1 { get; set; }
        public decimal ScoreSlowEmaLevel1 { get; set; }
        public decimal ScoreMediumEmaLevel1 { get; set; }
        public decimal ProbSlowEmaLevel1 { get; set; }
        public decimal ProbMediumEmaLevel1 { get; set; }
        public decimal ProbFastEmaLevel1 { get; set; }

        public decimal ScoreFastEmaLevel2 { get; set; }
        public decimal ScoreSlowEmaLevel2 { get; set; }
        public decimal ScoreMediumEmaLevel2 { get; set; }
        public decimal ProbSlowEmaLevel2 { get; set; }
        public decimal ProbMediumEmaLevel2 { get; set; }
        public decimal ProbFastEmaLevel2 { get; set; }

        public decimal ScoreFastEmaLevel3 { get; set; }
        public decimal ScoreSlowEmaLevel3 { get; set; }
        public decimal ScoreMediumEmaLevel3 { get; set; }
        public decimal ProbSlowEmaLevel3 { get; set; }
        public decimal ProbMediumEmaLevel3 { get; set; }
        public decimal ProbFastEmaLevel3 { get; set; }

        public int Level3 { get; set; }
        public int Level2 { get; set; }
        public int Level1 { get; set; }

        public StabilityDirection FastScoreTrend { get; set; }
        public StabilityDirection MediumScoreTrend { get; set; }
        public StabilityDirection SlowScoreTrend { get; set; }
        public StabilityDirection FastProbTrend { get; set; }
        public StabilityDirection MediumProbTrend { get; set; }
        public StabilityDirection SlowProbTrend { get; set; }

    }
}
