using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Pricing;

public static class CandleAggregator
{
    public static MinuteCandle Aggregate(
        IEnumerable<MinuteCandle> candles)
    {
        var list = candles.ToList();

        if (list.Count == 0)
            return new MinuteCandle();

        return new MinuteCandle
        {
            MinuteUtc = list.First().MinuteUtc,

            Open = list.First().Open,

            High = list.Max(x => x.High),

            Low = list.Min(x => x.Low),

            Close = list.Last().Close
        };
    }
}