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
    private readonly DepthFeatureManager _depthFeatureManager;
    private readonly ISnapshotRepository _snapshotRepository;

    public GptMarketSnapshotV2Builder(GptMultiTimeframeAggregator aggregator, DepthFeatureManager depthFeatureManager, ISnapshotRepository snapshotRepository)
    {
        _aggregator = aggregator;
        _depthFeatureManager = depthFeatureManager;
        _snapshotRepository = snapshotRepository;
    }

    public async Task<GptMarketSnapshotV2> Build(int sequence, string engineVersion, KinetixEngineResult result, MarketStructureSnapshot structure)
    {
        var mtf = _aggregator.Build();
        var depthMtf = new DepthMtfAggregator(_depthFeatureManager.Rows).Build();
        var snapshots = await _snapshotRepository.GetRecentSnapshotsAsync(3);
        var history = snapshots.OrderBy(x => x.Sequence).Select(HistoricalSnapshotMapper.Map).ToList();

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
            Depth = new GptDepthSnapshot
            {
                DepthImbalance = depthMtf.Imbalance,
                DepthBullPct = depthMtf.BullishPercent,
                BidWallAge = depthMtf.BidWallAge,
                AskWallAge = depthMtf.AskWallAge,
                BidWallQty = depthMtf.BidWallQty,
                AskWallQty = depthMtf.AskWallQty,
                BidConsumption = depthMtf.BidConsumption,
                BullishPersistence = depthMtf.BullishPersistence,
                AskConsumption = depthMtf.AskConsumption
            },
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

            for (int i = 0; i < history.Count; i++)
            {
                var current = history[i];
                var previous = i > 0 ? history[i - 1] : null;

                sb.AppendLine($"- Seq {current.Sequence}: Price = {current.Price}");

                if (previous != null)
                {
                    sb.AppendLine($"  → Price moved from {previous.Price} to {current.Price}");

                    // Trend changes
                    if (previous.TrendLevel1 != current.TrendLevel1)
                        sb.AppendLine($"  → Level1 trend changed from {previous.TrendLevel1} to {current.TrendLevel1}");

                    if (previous.TrendLevel2 != current.TrendLevel2)
                        sb.AppendLine($"  → Level2 trend changed from {previous.TrendLevel2} to {current.TrendLevel2}");

                    // ScoreZ trend
                    sb.AppendLine($"  → ScoreZ: L1={current.ScoreZLevel1:F3}, L2={current.ScoreZLevel2:F3}, L3={current.ScoreZLevel3:F3}");

                    // Momentum trend
                    sb.AppendLine($"  → Momentum: L1={current.MomentumLevel1:F4}, L2={current.MomentumLevel2:F4}");

                    // Persistence trend (highlight Level2 as it's important)
                    sb.AppendLine($"  → Persistence L2: {current.PersistenceLevel2:F2}");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}