using KinetixFlowEngine.Core.Database.Repositories;
using KinetixFlowEngine.Core.Depth;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Models;
using Serilog.Core;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptMarketSnapshotV2Builder
{
    private readonly GptMultiTimeframeAggregator _aggregator;

    public GptMarketSnapshotV2Builder(GptMultiTimeframeAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public async Task<GptMarketSnapshotV2> Build(int sequence, string engineVersion, KinetixEngineResult result, MarketStructureSnapshot structure, List<HistoricalSnapshotSummary> history)
    {
        var mtf = _aggregator.Build();
        //var depthMtf = new DepthMtfAggregator(_depthFeatureManager.Rows).Build();
        //var snapshots = await _snapshotRepository.GetRecentSnapshotsAsync(3);
        //var history = snapshots.OrderBy(x => x.Sequence).Select(HistoricalSnapshotMapper.Map).ToList();

        return new GptMarketSnapshotV2
        {
            Sequence = sequence,

            EngineVersion = engineVersion,

            SnapshotTimeUtc = DateTime.UtcNow,

            Price = (decimal)result.Price,

            VWAP = (decimal)result.VWAP,

            ATR15m = result.ATR15m,

            FundingRate = result.FundingRate,

            FundingPressure = result.FundingPressure,

            OIChange = result.OIChange,

            ScoreZ = mtf.ScoreZ,

            VelocityZ = mtf.VelocityZ,

            ImbalanceZ = mtf.ImbalanceZ,

            CompressionZ = mtf.CompressionZ,

            ExhaustionZ = mtf.ExhaustionZ,

            Momentum = mtf.Momentum,

            Acceleration = mtf.Acceleration,

            Persistence = mtf.Persistence,

            NetPressure = mtf.NetPressure,

            FlowImpactEfficiency = mtf.FlowImpactEfficiencyZ,

            ER5 = mtf.ER5,

            ER30 = mtf.ER30,
            //Depth = new GptDepthSnapshot
            //{
            //    DepthImbalance = depthMtf.Imbalance,
            //    DepthBullPct = depthMtf.BullishPercent,
            //    BidWallAge = depthMtf.BidWallAge,
            //    AskWallAge = depthMtf.AskWallAge,
            //    BidWallQty = depthMtf.BidWallQty,
            //    AskWallQty = depthMtf.AskWallQty,
            //    BidConsumption = depthMtf.BidConsumption,
            //    BullishPersistence = depthMtf.BullishPersistence,
            //    AskConsumption = depthMtf.AskConsumption
            //},
            TrendLevel1 = structure.Trend10m,
            TrendLevel2 = structure.Trend30m,
            TrendLevel3 = structure.Trend60m,

            DistanceFromLevel1High = structure.DistanceFrom10mHigh,
            DistanceFromLevel1Low = structure.DistanceFrom10mLow,

            DistanceFromLevel2High = structure.DistanceFrom30mHigh,
            DistanceFromLevel2Low = structure.DistanceFrom30mLow,

            DistanceFromLevel3High = structure.DistanceFrom60mHigh,
            DistanceFromLevel3Low = structure.DistanceFrom60mLow,

            DistanceFromVWAP = structure.DistanceFromVWAP,
            DistanceFromVWAPPct = structure.DistanceFromVWAPPct,

            BodyPctLevel1 = structure.Candle10m.BodyPct,
            UpperWickPctLevel1 = structure.Candle10m.UpperWickPct,
            LowerWickPctLevel1 = structure.Candle10m.LowerWickPct,

            BodyPctLevel2 = structure.Candle30m.BodyPct,
            UpperWickPctLevel2 = structure.Candle30m.UpperWickPct,
            LowerWickPctLevel2 = structure.Candle30m.LowerWickPct,

            BodyPctLevel3 = structure.Candle60m.BodyPct,
            UpperWickPctLevel3 = structure.Candle60m.UpperWickPct,
            LowerWickPctLevel3 = structure.Candle60m.LowerWickPct,

            HistorySummary = HistorySummaryGenerator.Generate(history)
        };
    }

    public static class HistorySummaryGenerator
    {
        public static string Generate(List<HistoricalSnapshotSummary> history)
        {
            if (history == null || history.Count == 0)
                return "No recent history available.";

            var sb = new StringBuilder();
            sb.AppendLine("Recent History Summary (Last 3 Snapshots):");

            // Process each snapshot with comparison to previous
            for (int i = 0; i < history.Count; i++)
            {
                var current = history[i];
                var previous = i > 0 ? history[i - 1] : null;

                sb.AppendLine($"- Seq {current.Sequence}: Price = {current.Price}");

                if (previous != null)
                {
                    // Price direction
                    string priceMove = current.Price > previous.Price ? "↑ up" : "↓ down";
                    sb.AppendLine($"  → Price moved {priceMove} ({previous.Price} → {current.Price})");

                    // === Trend Level Changes ===
                    if (previous.TrendLevel1 != current.TrendLevel1)
                        sb.AppendLine($"  → Level1 Structure: {previous.TrendLevel1} → {current.TrendLevel1}");
                    if (previous.TrendLevel2 != current.TrendLevel2)
                        sb.AppendLine($"  → Level2 Structure: {previous.TrendLevel2} → {current.TrendLevel2}");
                    if (previous.TrendLevel3 != current.TrendLevel3)
                        sb.AppendLine($"  → Level3 Structure: {previous.TrendLevel3} → {current.TrendLevel3}");

                    // === ScoreZ Trend Analysis ===
                    string scoreZTrend = GetTrendDescription(previous.ScoreZLevel2, current.ScoreZLevel2, "ScoreZ (Level2)");
                    sb.AppendLine($"  → {scoreZTrend}");

                    // === Momentum Trend Analysis ===
                    string momentumTrend = GetTrendDescription(previous.MomentumLevel2, current.MomentumLevel2, "Momentum (Level2)");
                    sb.AppendLine($"  → {momentumTrend}");

                    // === Persistence Trend (Important for trend strength) ===
                    string persistenceTrend = GetTrendDescription(previous.PersistenceLevel2, current.PersistenceLevel2, "Persistence (Level2)");
                    sb.AppendLine($"  → {persistenceTrend}");
                }
            }

            // === Overall Regime Observation ===
            sb.AppendLine();
            sb.AppendLine("Overall Recent Regime:");

            var latest = history.Last();
            var oldest = history.First();

            // Higher timeframe structure assessment
            if (latest.TrendLevel2 == "Bullish" && latest.TrendLevel3 == "Bullish")
            {
                sb.AppendLine("- Higher timeframe structure (Level2 & Level3) remains **Bullish**.");
            }
            else if (latest.TrendLevel2 == "Bearish" && latest.TrendLevel3 == "Bearish")
            {
                sb.AppendLine("- Higher timeframe structure (Level2 & Level3) is **Bearish**.");
            }
            else
            {
                sb.AppendLine("- Higher timeframe structure is **mixed** between Level2 and Level3.");
            }

            // Recent momentum & persistence strength
            if (latest.PersistenceLevel2 > 4.0)
            {
                sb.AppendLine("- Strong persistence on Level2 suggests the current trend has good durability.");
            }
            else if (latest.PersistenceLevel2 < 1.0)
            {
                sb.AppendLine("- Weak persistence on Level2 indicates the recent move lacks strong follow-through.");
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Helper method to describe trend strength between two values
        /// </summary>
        private static string GetTrendDescription(double previousValue, double currentValue, string metricName)
        {
            double change = currentValue - previousValue;
            double absChange = Math.Abs(change);

            string direction = change > 0 ? "improving" : "weakening";
            string strength = absChange switch
            {
                > 0.08 => "strongly",
                > 0.03 => "moderately",
                > 0.01 => "slightly",
                _ => "minimally"
            };

            return $"{metricName} is {strength} {direction} ({previousValue:F3} → {currentValue:F3})";
        }
    }
}