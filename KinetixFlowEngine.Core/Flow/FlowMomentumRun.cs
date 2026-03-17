using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowMomentumRun
    {
        private double _run = 0;

        private const double DECAY = 0.65;
        private const double MAX_RUN = 12;

        public decimal LastFactor { get; private set; }

        // ✅ NEW: expose run for persistence
        public double Run => _run;

        // ✅ NEW: restore state
        public void Restore(double run)
        {
            _run = Math.Clamp(run, -MAX_RUN, MAX_RUN);
        }

        public decimal GetFactor(double score, double velocityZ)
        {
            // ✅ FIX 3: correct scaling for probability (0–1 input)
            double normalized = Math.Abs(score - 0.5) * 2.0; // 0 → 1
            double strength = Math.Min(normalized, 1.0);

            if (score > 0.55)
                _run = Math.Min(MAX_RUN, _run + strength);
            else if (score < 0.45)
                _run = Math.Max(-MAX_RUN, _run - strength);
            else
                _run *= DECAY;

            if (Math.Abs(velocityZ) < 0.3)
                _run *= 0.75;

            double absRun = Math.Abs(_run);
            double normalizedRun = absRun / MAX_RUN;

            double factor = 0.18 + (0.30 * normalizedRun);

            LastFactor = (decimal)Math.Clamp(factor, 0.18, 0.48);

            return LastFactor;
        }

        // ✅ FIX 2: bootstrap
        public void Bootstrap(double scoreZ)
        {
            _run = Math.Clamp(scoreZ * 2.0, -6, 6);
        }

        public void Reset() => _run = 0;
    }
}
