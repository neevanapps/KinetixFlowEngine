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

        public decimal LastFactor { get; private set; }   // <-- ADD THIS

        public decimal GetFactor(double score, double velocityZ)
        {
            double absScore = Math.Abs(score);

            double strength = Math.Min(absScore / 2.5, 1.0);

            if (score > 0.4)
                _run = Math.Min(MAX_RUN, _run + strength);
            else if (score < -0.4)
                _run = Math.Max(-MAX_RUN, _run - strength);
            else
                _run *= DECAY;

            if (Math.Abs(velocityZ) < 0.3)
                _run *= 0.75;

            double absRun = Math.Abs(_run);
            double normalized = absRun / MAX_RUN;

            double factor = 0.18 + (0.30 * normalized);

            LastFactor = (decimal)Math.Clamp(factor, 0.18, 0.48); // <-- STORE

            return LastFactor;
        }

        public void Reset() => _run = 0;
    }
}
