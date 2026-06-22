using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Models
{
    public sealed class MarketStructureSnapshot
    {
        public string Trend10m { get; set; } = "";

        public string Trend30m { get; set; } = "";

        public string Trend60m { get; set; } = "";

        public double DistanceFrom10mHigh { get; set; }
        public double DistanceFrom10mLow { get; set; }

        public double DistanceFrom30mHigh { get; set; }
        public double DistanceFrom30mLow { get; set; }

        public double DistanceFrom60mHigh { get; set; }
        public double DistanceFrom60mLow { get; set; }

        public double DistanceFromVWAP { get; set; }

        public double DistanceFromVWAPPct { get; set; }

        public decimal RangeHigh10m { get; set; }
        public decimal RangeLow10m { get; set; }

        public decimal RangeHigh30m { get; set; }
        public decimal RangeLow30m { get; set; }

        public decimal RangeHigh60m { get; set; }
        public decimal RangeLow60m { get; set; }

        public CandleSnapshot Candle10m { get; set; } = new();

        public CandleSnapshot Candle30m { get; set; } = new();

        public CandleSnapshot Candle60m { get; set; } = new();
    }
}
