using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptSnapshotBuilder
{
    private const int RequiredSamples = 3;

    private AggregationState _state = new();

    public void Add(KinetixEngineResult result)
    {
        if (_state.SampleCount == 0)
        {
            _state.StartTimeUtc = DateTime.UtcNow;

            _state.Open = (decimal)result.Price;
            _state.High = (decimal)result.Price;
            _state.Low = (decimal)result.Price;

            _state.OIStart = result.OIChange;
        }

        var price = (decimal)result.Price;

        _state.Close = price;

        if (price > _state.High)
            _state.High = price;

        if (price < _state.Low)
            _state.Low = price;

        _state.EndTimeUtc = DateTime.UtcNow;

        _state.OIEnd = result.OIChange;

        _state.MomentumSum += result.Momentum;
        _state.AccelerationSum += result.Acceleration;
        _state.PersistenceSum += result.Persistence;
        _state.SizeBiasSum += result.SizeBias;
        _state.AbsorptionSum += result.Absorption;
        _state.DeltaVelocitySum += result.DeltaVelocity;

        _state.BuyPressureSum += result.BuyPressure;
        _state.SellPressureSum += result.SellPressure;
        _state.NetPressureSum += result.NetPressure;

        _state.FundingPressureSum += result.FundingPressure;

        _state.SampleCount++;

        _state.Last = result;
    }

    public bool IsReady()
    {
        return _state.SampleCount >= RequiredSamples;
    }

    public void Reset()
    {
        _state = new AggregationState();
    }

    public GptMarketSnapshot BuildSnapshot(
        int sequence,
        string engineVersion)
    {
        if (_state.SampleCount == 0)
            throw new InvalidOperationException("No samples collected.");

        if (_state.Last == null)
            throw new InvalidOperationException("Last sample missing.");

        var last = _state.Last;

        double avg(double value) => value / _state.SampleCount;

        return new GptMarketSnapshot
        {
            Sequence = sequence,
            EngineVersion = engineVersion,

            StartTimeUtc = _state.StartTimeUtc,
            EndTimeUtc = _state.EndTimeUtc,

            SampleCount = _state.SampleCount,

            Open = _state.Open,
            High = _state.High,
            Low = _state.Low,
            Close = _state.Close,

            VWAP = last.VWAP,
            ATR15m = last.ATR15m,

            OIStart = _state.OIStart,
            OIEnd = _state.OIEnd,

            FundingRate = last.FundingRate,
            FundingPressure = avg(_state.FundingPressureSum),

            RawScore = last.RawScore,
            AdjustedScore = last.AdjustedScore,

            ScoreZ = last.ScoreZ,
            VelocityZ = last.VelocityZ,
            ImbalanceZ = last.ImbalanceZ,
            CompressionZ = last.CompressionZ,
            ExhaustionZ = last.ExhaustionZ,

            MomentumAvg = avg(_state.MomentumSum),
            AccelerationAvg = avg(_state.AccelerationSum),
            PersistenceAvg = avg(_state.PersistenceSum),
            SizeBiasAvg = avg(_state.SizeBiasSum),
            AbsorptionAvg = avg(_state.AbsorptionSum),
            DeltaVelocityAvg = avg(_state.DeltaVelocitySum),

            BuyPressureAvg = avg(_state.BuyPressureSum),
            SellPressureAvg = avg(_state.SellPressureSum),
            NetPressureAvg = avg(_state.NetPressureSum),

            LargeBuyTrades = last.LargeBuyTrades,
            LargeSellTrades = last.LargeSellTrades,

            BuyClusterStrength = last.BuyClusterStrength,
            SellClusterStrength = last.SellClusterStrength,

            FlowImpactEfficiency = last.FlowImpactEfficiency,

            Volume15 = last.Volume15,

            ER5 = last.ER5,
            ER30 = last.ER30
        };
    }
}

