using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Pricing;

public sealed class PriceBuilder
{
    public Price Build(
        MinuteCandle candle,
        decimal vwap,
        decimal atr,
        decimal previousClose)
    {
        return new Price
        {
            Candle = candle,

            VWAP = vwap,

            ATR = atr,

            DistanceFromVWAP = BuildMetric(
                candle.Close - vwap),

            DistanceFromPreviousClose = BuildMetric(
                candle.Close - previousClose)
        };
    }

    private static MetricState BuildMetric(decimal value)
    {
        return new MetricState
        {
            Open = value,
            High = value,
            Low = value,
            Close = value
        };
    }
}