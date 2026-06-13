using KinetixFlowEngine.Core.Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class AggregationState
    {
        public int SampleCount;

        public DateTime StartTimeUtc;
        public DateTime EndTimeUtc;

        public decimal Open;
        public decimal High;
        public decimal Low;
        public decimal Close;

        public double OIStart;
        public double OIEnd;

        // Running sums
        public double MomentumSum;
        public double AccelerationSum;
        public double PersistenceSum;
        public double SizeBiasSum;
        public double AbsorptionSum;
        public double DeltaVelocitySum;

        public double BuyPressureSum;
        public double SellPressureSum;
        public double NetPressureSum;

        public double FundingPressureSum;

        // Last values
        public KinetixEngineResult? Last;
    }
}
