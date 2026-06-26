namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketQualityBuilder
{
    public byte Build(MarketState state)
    {
        byte score = 100;

        if (state.Freshness.TradeAge.TotalSeconds > 5)
            score -= 20;

        if (state.Freshness.DepthAge.TotalSeconds > 5)
            score -= 20;

        if (state.Freshness.OIAge.TotalMinutes > 2)
            score -= 20;

        if (state.Freshness.FundingAge.TotalMinutes > 2)
            score -= 20;

        if (state.Depth == null)
            score -= 20;

        return score;
    }
}