using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KinetixFlowEngine.Core.Database
{
    public class MarketSnapshotEntity
    {
        [Key]
        public long Id { get; set; }

        public int Sequence { get; set; }

        public string EngineVersion { get; set; } = string.Empty;

        public DateTime SnapshotTimeUtc { get; set; }

        public DateTime CreatedUtc { get; set; }

        public decimal Price { get; set; }

        public decimal VWAP { get; set; }

        public double ATR15m { get; set; }

        public double FundingRate { get; set; }

        public double FundingPressure { get; set; }

        public double OIChange { get; set; }

        // -----------------------------
        // Score
        // -----------------------------

        public double ScoreZ10m { get; set; }
        public double ScoreZ30m { get; set; }
        public double ScoreZ60m { get; set; }

        public double VelocityZ10m { get; set; }
        public double VelocityZ30m { get; set; }
        public double VelocityZ60m { get; set; }

        public double ImbalanceZ10m { get; set; }
        public double ImbalanceZ30m { get; set; }
        public double ImbalanceZ60m { get; set; }

        public double CompressionZ10m { get; set; }
        public double CompressionZ30m { get; set; }
        public double CompressionZ60m { get; set; }

        public double ExhaustionZ10m { get; set; }
        public double ExhaustionZ30m { get; set; }
        public double ExhaustionZ60m { get; set; }

        // -----------------------------
        // Flow
        // -----------------------------

        public double Momentum10m { get; set; }
        public double Momentum30m { get; set; }
        public double Momentum60m { get; set; }

        public double Acceleration10m { get; set; }
        public double Acceleration30m { get; set; }
        public double Acceleration60m { get; set; }

        public double Persistence10m { get; set; }
        public double Persistence30m { get; set; }
        public double Persistence60m { get; set; }

        public double NetPressure10m { get; set; }
        public double NetPressure30m { get; set; }
        public double NetPressure60m { get; set; }

        public double FlowImpact10m { get; set; }
        public double FlowImpact30m { get; set; }
        public double FlowImpact60m { get; set; }

        // -----------------------------
        // Efficiency Ratio
        // -----------------------------

        public double ER5_10m { get; set; }
        public double ER5_30m { get; set; }
        public double ER5_60m { get; set; }

        public double ER30_10m { get; set; }
        public double ER30_30m { get; set; }
        public double ER30_60m { get; set; }

        // -----------------------------
        // Depth
        // -----------------------------

        public double DepthImbalance10m { get; set; }
        public double DepthImbalance30m { get; set; }
        public double DepthImbalance60m { get; set; }

        public double DepthBullPct10m { get; set; }
        public double DepthBullPct30m { get; set; }
        public double DepthBullPct60m { get; set; }

        public double BidWallAge10m { get; set; }
        public double BidWallAge30m { get; set; }
        public double BidWallAge60m { get; set; }

        public double AskWallAge10m { get; set; }
        public double AskWallAge30m { get; set; }
        public double AskWallAge60m { get; set; }

        public double BidWallQty10m { get; set; }
        public double BidWallQty30m { get; set; }
        public double BidWallQty60m { get; set; }

        public double AskWallQty10m { get; set; }
        public double AskWallQty30m { get; set; }
        public double AskWallQty60m { get; set; }

        public ICollection<ModelReviewEntity> Reviews { get; set; }
            = new List<ModelReviewEntity>();
    }
}
