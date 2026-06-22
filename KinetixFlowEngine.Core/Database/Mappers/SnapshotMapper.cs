using KinetixFlowEngine.Core.Gpt.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Database.Mappers
{
    public static class SnapshotMapper
    {
        public static MarketSnapshotEntity Map(
            GptMarketSnapshotV2 s)
        {
            return new MarketSnapshotEntity
            {
                Sequence = s.Sequence,
                EngineVersion = s.EngineVersion,
                SnapshotTimeUtc = s.SnapshotTimeUtc,
                CreatedUtc = DateTime.UtcNow,

                Price = s.Price,
                VWAP = s.VWAP,
                ATR15m = s.ATR15m,

                FundingRate = s.FundingRate,
                FundingPressure = s.FundingPressure,
                OIChange = s.OIChange,

                ScoreZ10m = s.ScoreZ[0],
                ScoreZ30m = s.ScoreZ[1],
                ScoreZ60m = s.ScoreZ[2],

                VelocityZ10m = s.VelocityZ[0],
                VelocityZ30m = s.VelocityZ[1],
                VelocityZ60m = s.VelocityZ[2],

                ImbalanceZ10m = s.ImbalanceZ[0],
                ImbalanceZ30m = s.ImbalanceZ[1],
                ImbalanceZ60m = s.ImbalanceZ[2],

                CompressionZ10m = s.CompressionZ[0],
                CompressionZ30m = s.CompressionZ[1],
                CompressionZ60m = s.CompressionZ[2],

                ExhaustionZ10m = s.ExhaustionZ[0],
                ExhaustionZ30m = s.ExhaustionZ[1],
                ExhaustionZ60m = s.ExhaustionZ[2],

                Momentum10m = s.Momentum[0],
                Momentum30m = s.Momentum[1],
                Momentum60m = s.Momentum[2],

                Acceleration10m = s.Acceleration[0],
                Acceleration30m = s.Acceleration[1],
                Acceleration60m = s.Acceleration[2],

                Persistence10m = s.Persistence[0],
                Persistence30m = s.Persistence[1],
                Persistence60m = s.Persistence[2],

                NetPressure10m = s.NetPressure[0],
                NetPressure30m = s.NetPressure[1],
                NetPressure60m = s.NetPressure[2],

                FlowImpact10m = s.FlowImpactEfficiency[0],
                FlowImpact30m = s.FlowImpactEfficiency[1],
                FlowImpact60m = s.FlowImpactEfficiency[2],

                ER5_10m = s.ER5[0],
                ER5_30m = s.ER5[1],
                ER5_60m = s.ER5[2],

                ER30_10m = s.ER30[0],
                ER30_30m = s.ER30[1],
                ER30_60m = s.ER30[2],

                DepthImbalance10m = s.Depth.DepthImbalance[0],
                DepthImbalance30m = s.Depth.DepthImbalance[1],
                DepthImbalance60m = s.Depth.DepthImbalance[2],

                DepthBullPct10m = s.Depth.DepthBullPct[0],
                DepthBullPct30m = s.Depth.DepthBullPct[1],
                DepthBullPct60m = s.Depth.DepthBullPct[2],

                BidWallAge10m = s.Depth.BidWallAge[0],
                BidWallAge30m = s.Depth.BidWallAge[1],
                BidWallAge60m = s.Depth.BidWallAge[2],

                AskWallAge10m = s.Depth.AskWallAge[0],
                AskWallAge30m = s.Depth.AskWallAge[1],
                AskWallAge60m = s.Depth.AskWallAge[2],

                BidWallQty10m = s.Depth.BidWallQty[0],
                BidWallQty30m = s.Depth.BidWallQty[1],
                BidWallQty60m = s.Depth.BidWallQty[2],

                AskWallQty10m = s.Depth.AskWallQty[0],
                AskWallQty30m = s.Depth.AskWallQty[1],
                AskWallQty60m = s.Depth.AskWallQty[2],

                BullishPersistence10m = s.Depth.BullishPersistence[0],
                BullishPersistence30m = s.Depth.BullishPersistence[1],
                BullishPersistence60m = s.Depth.BullishPersistence[2],

                BidConsumption10m = s.Depth.BidConsumption[0],
                BidConsumption30m = s.Depth.BidConsumption[1],
                BidConsumption60m = s.Depth.BidConsumption[2],

                AskConsumption10m = s.Depth.AskConsumption[0],
                AskConsumption30m = s.Depth.AskConsumption[1],
                AskConsumption60m = s.Depth.AskConsumption[2],

                Trend10m = s.Trend10m,
                Trend30m = s.Trend30m,
                Trend60m = s.Trend60m,

                DistanceFrom10mHigh = s.DistanceFrom10mHigh,
                DistanceFrom10mLow = s.DistanceFrom10mLow,

                DistanceFrom30mHigh = s.DistanceFrom30mHigh,
                DistanceFrom30mLow = s.DistanceFrom30mLow,

                DistanceFrom60mHigh = s.DistanceFrom60mHigh,
                DistanceFrom60mLow = s.DistanceFrom60mLow,

                DistanceFromVWAP = s.DistanceFromVWAP,
                DistanceFromVWAPPct = s.DistanceFromVWAPPct,

                Open10m = s.Open10m,
                High10m = s.High10m,
                Low10m = s.Low10m,
                Close10m = s.Close10m,

                Open30m = s.Open30m,
                High30m = s.High30m,
                Low30m = s.Low30m,
                Close30m = s.Close30m,

                Open60m = s.Open60m,
                High60m = s.High60m,
                Low60m = s.Low60m,
                Close60m = s.Close60m,

                BodyPct10m = s.BodyPct10m,
                UpperWickPct10m = s.UpperWickPct10m,
                LowerWickPct10m = s.LowerWickPct10m,

                BodyPct30m = s.BodyPct30m,
                UpperWickPct30m = s.UpperWickPct30m,
                LowerWickPct30m = s.LowerWickPct30m,

                BodyPct60m = s.BodyPct60m,
                UpperWickPct60m = s.UpperWickPct60m,
                LowerWickPct60m = s.LowerWickPct60m,

                RangeHigh10m = s.RangeHigh10m,
                RangeLow10m = s.RangeLow10m,

                RangeHigh30m = s.RangeHigh30m,
                RangeLow30m = s.RangeLow30m,

                RangeHigh60m = s.RangeHigh60m,
                RangeLow60m = s.RangeLow60m,

            };
        }
    }
}
