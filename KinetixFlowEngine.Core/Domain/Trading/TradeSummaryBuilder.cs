using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Trading;

public sealed class TradeSummaryBuilder
{
    public DomainSummary Build(Trade trade)
    {
        var bias = GetBias(trade);

        var strength = GetStrength(trade);

        var narrative = BuildNarrative(trade);

        return new DomainSummary
        {
            Bias = bias,
            Strength = strength,
            Narrative = narrative
        };
    }

    private static MarketBias GetBias(Trade trade)
    {
        if (trade.Volume.Delta.Close > 0)
            return MarketBias.Bullish;

        if (trade.Volume.Delta.Close < 0)
            return MarketBias.Bearish;

        return MarketBias.Neutral;
    }

    private static MarketStrength GetStrength(Trade trade)
    {
        var delta = Math.Abs(trade.Volume.Delta.Close);

        if (delta > 100m)
            return MarketStrength.Extreme;

        if (delta > 50m)
            return MarketStrength.Strong;

        if (delta > 20m)
            return MarketStrength.Moderate;

        return MarketStrength.Weak;
    }

    private static string BuildNarrative(Trade trade)
    {
        var direction =
            trade.Volume.Delta.Close >= 0
                ? "buyers"
                : "sellers";

        return
            $"Net traded volume favored {direction}. " +
            $"Buy Volume: {trade.Volume.Buy.Close:F2} BTC, " +
            $"Sell Volume: {trade.Volume.Sell.Close:F2} BTC, " +
            $"Delta: {trade.Volume.Delta.Close:F2} BTC.";
    }
}