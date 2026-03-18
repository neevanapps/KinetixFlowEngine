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

        public double Run => _run;

        public void Restore(double run)
        {
            _run = Math.Clamp(run, -MAX_RUN, MAX_RUN);
        }

        public decimal GetFactor(double score, double velocityZ)
        {
            double absScore = Math.Abs(score);
            double strength = Math.Min(absScore / 2.2, 1.0);   // was /2.5 → slightly faster build

            if (score > 0.35)                                  // lowered from 0.4
                _run = Math.Min(MAX_RUN, _run + strength * 1.05);   // slight bullish bias if you want
            else if (score < -0.35)                            // lowered from -0.4
                _run = Math.Max(-MAX_RUN, _run - strength * 1.15); // faster bearish build (important!)
            else
                _run *= 0.58;                                  // was 0.65 → faster normal decay

            // Stronger momentum-based decay
            if (Math.Abs(velocityZ) < 0.4)                     // was 0.3
                _run *= 0.58;                                  // was 0.75

            double absRun = Math.Abs(_run);
            double normalized = absRun / MAX_RUN;
            double factor = 0.18 + (0.26 * normalized);        // was 0.30 → softer max

            LastFactor = (decimal)Math.Clamp(factor, 0.18, 0.46);  // hard cap lowered to 0.46
            return LastFactor;
        }

        public void Bootstrap(double scoreZ)
        {
            _run = Math.Clamp(scoreZ * 2.0, -6, 6);
        }

        public void Reset() => _run = 0;
    }
}
