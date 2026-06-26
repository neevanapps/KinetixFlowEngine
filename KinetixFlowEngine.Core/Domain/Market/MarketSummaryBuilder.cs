using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketSummaryBuilder
{
    public DomainSummary Build(MarketState state)
    {
        var bias = state.Price.Summary.Bias;

        if (state.Trade.Summary.Bias == state.OpenInterest.Summary.Bias)
            bias = state.Trade.Summary.Bias;

        var strength =
            Max(
                state.Price.Summary.Strength,
                state.Trade.Summary.Strength,
                state.OpenInterest.Summary.Strength,
                state.Funding.Summary.Strength);

        return new DomainSummary
        {
            Bias = bias,

            Strength = strength,

            Narrative =
                $"Regime: {state.Regime}. Price={state.Price.Summary.Bias}, Trade={state.Trade.Summary.Bias}, OI={state.OpenInterest.Summary.Bias}, Funding={state.Funding.Summary.Bias}."
        };
    }

    private static MarketStrength Max(params MarketStrength[] strengths)
    {
        return strengths.Max();
    }
}