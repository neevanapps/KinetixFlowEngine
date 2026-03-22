using KinetixFlowEngine.Core.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowMomentumRun
    {
        private double _run = 0;
        private const double MAX_RUN = 12.0;
        private const double BUILD_MULTIPLIER = 1.10;   // SAME for long and short → NEUTRAL
        private const double DECAY = 0.62;              // slightly softer decay
        private const double VOLUME_BOOST = 1.30;       // when volume expands
        private const double VOLUME_DAMP = 0.78;        // when volume contracts

        public decimal LastFactor { get; private set; }
        public double Run => _run;

        private readonly VolumeEngine _volumeEngine;

        public FlowMomentumRun(VolumeEngine volumeEngine)
        {
            _volumeEngine = volumeEngine;
        }

        public void Restore(double run)
        {
            _run = Math.Clamp(run, -MAX_RUN, MAX_RUN);
        }

        public decimal GetFactor(double normalizedInput, double velocityZ)
        {
            double absInput = Math.Abs(normalizedInput);
            double strength = Math.Min(absInput / 2.0, 1.0);   // balanced divisor

            // === VOLUME AWARENESS (your idea) ===
            double volumeFactor = 1.0;
            if (_volumeEngine != null)
            {
                bool isExpanding = _volumeEngine.IsVolumeExpansion(1.25); // 25% above average
                volumeFactor = isExpanding ? VOLUME_BOOST : VOLUME_DAMP;
            }

            // === NEUTRAL MOMENTUM BUILD ===
            if (normalizedInput > 0.32)
                _run = Math.Min(MAX_RUN, _run + strength * BUILD_MULTIPLIER * volumeFactor);
            else if (normalizedInput < -0.32)
                _run = Math.Max(-MAX_RUN, _run - strength * BUILD_MULTIPLIER * volumeFactor);
            else
                _run *= DECAY;

            // Extra decay on low velocity (neutral)
            if (Math.Abs(velocityZ) < 0.45)
                _run *= 0.65;

            // Final factor calculation
            double absRun = Math.Abs(_run);
            double normalized = absRun / MAX_RUN;
            double factor = 0.20 + (0.28 * normalized);   // clean neutral range

            LastFactor = (decimal)Math.Clamp(factor, 0.20, 0.48);
            return LastFactor;
        }

        public void Bootstrap(double input)
        {
            _run = Math.Clamp(input * 2.0, -6, 6);
        }

        public void Reset() => _run = 0;
    }
}
