using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.OI;

public sealed class OISummaryBuilder
{
    public DomainSummary Build(OpenInterest oi)
    {
        var bias = MarketBias.Neutral;

        if (oi.Change.Close > 0)
            bias = MarketBias.Bullish;

        if (oi.Change.Close < 0)
            bias = MarketBias.Bearish;

        var strength = MarketStrength.Weak;

        var absChange = Math.Abs(oi.Change.Close);

        if (absChange >= 100_000_000m)
            strength = MarketStrength.Extreme;
        else if (absChange >= 25_000_000m)
            strength = MarketStrength.Strong;
        else if (absChange >= 5_000_000m)
            strength = MarketStrength.Moderate;

        return new DomainSummary
        {
            Bias = bias,

            Strength = strength,

            Narrative =
                $"Open Interest changed by {oi.Change.Close:N0}."
        };
    }
}