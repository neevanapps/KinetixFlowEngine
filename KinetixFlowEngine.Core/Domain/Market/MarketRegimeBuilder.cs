using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketRegimeBuilder
{
    public MarketRegime Build(MarketState state)
    {
        var priceBull =
            state.Price.Summary.Bias == MarketBias.Bullish;

        var tradeBull =
            state.Trade.Summary.Bias == MarketBias.Bullish;

        var oiBull =
            state.OpenInterest.Summary.Bias == MarketBias.Bullish;

        var fundingBull =
            state.Funding.Summary.Bias == MarketBias.Bullish;

        var bullishVotes = 0;
        var bearishVotes = 0;

        if (priceBull) bullishVotes++; else bearishVotes++;
        if (tradeBull) bullishVotes++; else bearishVotes++;
        if (oiBull) bullishVotes++; else bearishVotes++;
        if (fundingBull) bullishVotes++; else bearishVotes++;

        if (bullishVotes >= 3)
            return MarketRegime.TrendingBull;

        if (bearishVotes >= 3)
            return MarketRegime.TrendingBear;

        if (state.Price.Summary.Strength == MarketStrength.Weak &&
            state.Trade.Summary.Strength == MarketStrength.Weak)
            return MarketRegime.Range;

        return MarketRegime.Transition;
    }
}