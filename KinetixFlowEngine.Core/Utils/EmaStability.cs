using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Utils
{
    public class EmaStability
    {
        private readonly FlowMomentumRun _momentumRun;
        private const int _minTicks = 12;

        // =========================
        // STATEFUL EMAs (18 total)
        // =========================
        // Score
        private readonly AdaptiveEma _sfL1 = new();
        private readonly AdaptiveEma _sfL2 = new();
        private readonly AdaptiveEma _sfL3 = new();
        private readonly AdaptiveEma _smL1 = new();
        private readonly AdaptiveEma _smL2 = new();
        private readonly AdaptiveEma _smL3 = new();
        private readonly AdaptiveEma _ssL1 = new();
        private readonly AdaptiveEma _ssL2 = new();
        private readonly AdaptiveEma _ssL3 = new();

        // Probability
        private readonly AdaptiveEma _pfL1 = new();
        private readonly AdaptiveEma _pfL2 = new();
        private readonly AdaptiveEma _pfL3 = new();
        private readonly AdaptiveEma _pmL1 = new();
        private readonly AdaptiveEma _pmL2 = new();
        private readonly AdaptiveEma _pmL3 = new();
        private readonly AdaptiveEma _psL1 = new();
        private readonly AdaptiveEma _psL2 = new();
        private readonly AdaptiveEma _psL3 = new();

        public EmaStability(FlowMomentumRun momentumRun)   // ← removed unused AtrNormalizer
        {
            _momentumRun = momentumRun;
        }

        public EmaStabilityState Compute(
            decimal scoreFast,
            decimal scoreMedium,
            decimal scoreSlow,
            decimal probFast,
            decimal probMedium,
            decimal probSlow,
            double atrNorm)   // ← you are correctly passing atrNorm
        {
            double momentum = Math.Clamp((double)_momentumRun.LastFactor, 0.2, 0.48);
            double momentumNorm = (momentum - 0.2) / (0.48 - 0.2);

            double regime = 0.7 * atrNorm + 0.3 * momentumNorm;
            regime = Math.Clamp(regime, 0.0, 1.0);

            int level1 = (int)Lerp(15 * _minTicks, 30 * _minTicks, regime);
            int level2 = (int)Lerp(30 * _minTicks, 60 * _minTicks, regime);
            int level3 = (int)Lerp(60 * _minTicks, 120 * _minTicks, regime);

            decimal factor = 0.3m + (decimal)regime * 0.7m;

            // SCORE EMA (now truly adaptive)
            decimal sf1 = _sfL1.UpdateWithFactor(scoreFast, factor, level1, level1);
            decimal sf2 = _sfL2.UpdateWithFactor(scoreFast, factor, level2, level2);
            decimal sf3 = _sfL3.UpdateWithFactor(scoreFast, factor, level3, level3);
            decimal sm1 = _smL1.UpdateWithFactor(scoreMedium, factor, level1, level1);
            decimal sm2 = _smL2.UpdateWithFactor(scoreMedium, factor, level2, level2);
            decimal sm3 = _smL3.UpdateWithFactor(scoreMedium, factor, level3, level3);
            decimal ss1 = _ssL1.UpdateWithFactor(scoreSlow, factor, level1, level1);
            decimal ss2 = _ssL2.UpdateWithFactor(scoreSlow, factor, level2, level2);
            decimal ss3 = _ssL3.UpdateWithFactor(scoreSlow, factor, level3, level3);

            decimal probFastC = probFast - 0.5m;
            decimal probMediumC = probMedium - 0.5m;
            decimal probSlowC = probSlow - 0.5m;
            // PROBABILITY EMA
            decimal pf1 = _pfL1.UpdateWithFactor(probFastC, factor, level1, level1);
            decimal pf2 = _pfL2.UpdateWithFactor(probFastC, factor, level2, level2);
            decimal pf3 = _pfL3.UpdateWithFactor(probFastC, factor, level3, level3);
            decimal pm1 = _pmL1.UpdateWithFactor(probMediumC, factor, level1, level1);
            decimal pm2 = _pmL2.UpdateWithFactor(probMediumC, factor, level2, level2);
            decimal pm3 = _pmL3.UpdateWithFactor(probMediumC, factor, level3, level3);
            decimal ps1 = _psL1.UpdateWithFactor(probSlowC, factor, level1, level1);
            decimal ps2 = _psL2.UpdateWithFactor(probSlowC, factor, level2, level2);
            decimal ps3 = _psL3.UpdateWithFactor(probSlowC, factor, level3, level3);

            var state = new EmaStabilityState
            {
                Regime = regime,
                Level1 = level1,
                Level2 = level2,
                Level3 = level3,

                ScoreFastEmaLevel1 = Math.Clamp(sf1, -50m, 50m),
                ScoreFastEmaLevel2 = Math.Clamp(sf2, -50m, 50m),
                ScoreFastEmaLevel3 = Math.Clamp(sf3, -50m, 50m),
                ScoreMediumEmaLevel1 = Math.Clamp(sm1, -50m, 50m),
                ScoreMediumEmaLevel2 = Math.Clamp(sm2, -50m, 50m),
                ScoreMediumEmaLevel3 = Math.Clamp(sm3, -50m, 50m),
                ScoreSlowEmaLevel1 = Math.Clamp(ss1, -50m, 50m),
                ScoreSlowEmaLevel2 = Math.Clamp(ss2, -50m, 50m),
                ScoreSlowEmaLevel3 = Math.Clamp(ss3, -50m, 50m),

                ProbFastEmaLevel1 = Math.Clamp(pf1, -50m, 50m),
                ProbFastEmaLevel2 = Math.Clamp(pf2, -50m, 50m),
                ProbFastEmaLevel3 = Math.Clamp(pf3, -50m, 50m),
                ProbMediumEmaLevel1 = Math.Clamp(pm1, -50m, 50m),
                ProbMediumEmaLevel2 = Math.Clamp(pm2, -50m, 50m),
                ProbMediumEmaLevel3 = Math.Clamp(pm3, -50m, 50m),
                ProbSlowEmaLevel1 = Math.Clamp(ps1, -50m, 50m),
                ProbSlowEmaLevel2 = Math.Clamp(ps2, -50m, 50m),
                ProbSlowEmaLevel3 = Math.Clamp(ps3, -50m, 50m),
            };

            state.FastScoreTrend = DetermineTrend(state.ScoreFastEmaLevel1, state.ScoreFastEmaLevel2, state.ScoreFastEmaLevel3);
            state.MediumScoreTrend = DetermineTrend(state.ScoreMediumEmaLevel1, state.ScoreMediumEmaLevel2, state.ScoreMediumEmaLevel3);
            state.SlowScoreTrend = DetermineTrend(state.ScoreSlowEmaLevel1, state.ScoreSlowEmaLevel2, state.ScoreSlowEmaLevel3);
            state.FastProbTrend = DetermineTrend(state.ProbFastEmaLevel1, state.ProbFastEmaLevel2, state.ProbFastEmaLevel3);
            state.MediumProbTrend = DetermineTrend(state.ProbMediumEmaLevel1, state.ProbMediumEmaLevel2, state.ProbMediumEmaLevel3);
            state.SlowProbTrend = DetermineTrend(state.ProbSlowEmaLevel1, state.ProbSlowEmaLevel2, state.ProbSlowEmaLevel3);

            return state;
        }

        private const decimal EPS = 0.0001m;

        private static StabilityDirection DetermineTrend(decimal l1, decimal l2, decimal l3)
        {
            bool up = (l1 - l2) > EPS && (l2 - l3) > EPS;
            bool down = (l2 - l1) > EPS && (l3 - l2) > EPS;

            // All positive → Long
            if (l1 > 0 && l2 > 0 && l3 > 0 && up)
                return StabilityDirection.Long;

            // All negative → Short
            if (l1 < 0 && l2 < 0 && l3 < 0 && down)
                return StabilityDirection.Short;

            return StabilityDirection.Neutral;
        }

        private static double Lerp(double min, double max, double t)
        {
            return min + (max - min) * Math.Clamp(t, 0.0, 1.0);
        }

        public void Restore(EmaStabilityState state)
        {
            // (unchanged - your Restore is already perfect)
            _sfL1.Restore(state.ScoreFastEmaLevel1);
            _sfL2.Restore(state.ScoreFastEmaLevel2);
            _sfL3.Restore(state.ScoreFastEmaLevel3);
            _smL1.Restore(state.ScoreMediumEmaLevel1);
            _smL2.Restore(state.ScoreMediumEmaLevel2);
            _smL3.Restore(state.ScoreMediumEmaLevel3);
            _ssL1.Restore(state.ScoreSlowEmaLevel1);
            _ssL2.Restore(state.ScoreSlowEmaLevel2);
            _ssL3.Restore(state.ScoreSlowEmaLevel3);

            _pfL1.Restore(state.ProbFastEmaLevel1);
            _pfL2.Restore(state.ProbFastEmaLevel2);
            _pfL3.Restore(state.ProbFastEmaLevel3);
            _pmL1.Restore(state.ProbMediumEmaLevel1);
            _pmL2.Restore(state.ProbMediumEmaLevel2);
            _pmL3.Restore(state.ProbMediumEmaLevel3);
            _psL1.Restore(state.ProbSlowEmaLevel1);
            _psL2.Restore(state.ProbSlowEmaLevel2);
            _psL3.Restore(state.ProbSlowEmaLevel3);
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
        public double Regime { get; set; }

        public StabilityDirection FastScoreTrend { get; set; }
        public StabilityDirection MediumScoreTrend { get; set; }
        public StabilityDirection SlowScoreTrend { get; set; }
        public StabilityDirection FastProbTrend { get; set; }
        public StabilityDirection MediumProbTrend { get; set; }
        public StabilityDirection SlowProbTrend { get; set; }

    }
}

