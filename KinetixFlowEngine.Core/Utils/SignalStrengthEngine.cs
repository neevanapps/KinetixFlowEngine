using KinetixFlowEngine.Core.Engine;

namespace KinetixFlowEngine.Core.Utils
{
    public class SignalStrengthEngine
    {
        private double? _prevStrength;

        public double LastStrength { get; private set; }
        public double Delta { get; private set; }

        public double Update(KinetixEngineResult r)
        {
            // =========================
            // FAST
            // =========================
            double f1 = (double)r.EmaStability.ScoreFastEmaLevel1;
            double f2 = (double)r.EmaStability.ScoreFastEmaLevel2;
            double f3 = (double)r.EmaStability.ScoreFastEmaLevel3;

            double fast =
                0.5 * f1 +
                0.3 * f2 +
                0.2 * f3;

            // =========================
            // MEDIUM
            // =========================
            double m1 = (double)r.EmaStability.ScoreMediumEmaLevel1;
            double m2 = (double)r.EmaStability.ScoreMediumEmaLevel2;
            double m3 = (double)r.EmaStability.ScoreMediumEmaLevel3;

            double medium =
                0.5 * m1 +
                0.3 * m2 +
                0.2 * m3;

            // =========================
            // SLOW
            // =========================
            double s1 = (double)r.EmaStability.ScoreSlowEmaLevel1;
            double s2 = (double)r.EmaStability.ScoreSlowEmaLevel2;
            double s3 = (double)r.EmaStability.ScoreSlowEmaLevel3;

            double slow =
                0.5 * s1 +
                0.3 * s2 +
                0.2 * s3;

            // =========================
            // TOTAL STRENGTH
            // =========================
            double strength =
                0.5 * fast +
                0.3 * medium +
                0.2 * slow;

            LastStrength = strength;

            // =========================
            // DELTA
            // =========================
            if (_prevStrength.HasValue)
                Delta = strength - _prevStrength.Value;
            else
                Delta = 0;

            _prevStrength = strength;

            return strength;
        }
    }
}