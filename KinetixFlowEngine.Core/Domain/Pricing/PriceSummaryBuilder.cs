using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Pricing;

public sealed class PriceSummaryBuilder
{
    public DomainSummary Build(Price price)
    {
        var bias = MarketBias.Neutral;

        if (price.Candle.Close > price.VWAP)
            bias = MarketBias.Bullish;

        if (price.Candle.Close < price.VWAP)
            bias = MarketBias.Bearish;

        var strength = MarketStrength.Weak;

        if (price.Candle.BodyPct > 60)
            strength = MarketStrength.Strong;
        else if (price.Candle.BodyPct > 30)
            strength = MarketStrength.Moderate;

        return new DomainSummary
        {
            Bias = bias,

            Strength = strength,

            Narrative =
                $"Price closed {(price.Candle.Close >= price.VWAP ? "above" : "below")} VWAP with {strength.ToString().ToLower()} conviction."
        };
    }
}