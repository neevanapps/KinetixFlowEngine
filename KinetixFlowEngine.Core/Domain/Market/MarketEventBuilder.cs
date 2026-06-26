using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketEventBuilder
{
    public IReadOnlyList<MarketEvent> Build(MarketState state)
    {
        var events = new List<MarketEvent>();

        events.AddRange(state.Price.Events);

        events.AddRange(state.Trade.Events);

        events.AddRange(state.Depth.Events);

        events.AddRange(state.OpenInterest.Events);

        events.AddRange(state.Funding.Events);

        //------------------------------------------------------
        // Cross-domain events
        //------------------------------------------------------

        if (state.Price.Summary.Bias == MarketBias.Bullish &&
            state.OpenInterest.Summary.Bias == MarketBias.Bullish)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = state.TimestampUtc,

                Type = MarketEventType.Unknown,

                Code = "NEW_LONGS",

                Description = "Price rising while Open Interest is increasing.",

                Strength = MarketStrength.Strong,

                Value = state.OpenInterest.Change.Close
            });
        }

        if (state.Price.Summary.Bias == MarketBias.Bearish &&
            state.OpenInterest.Summary.Bias == MarketBias.Bearish)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = state.TimestampUtc,

                Type = MarketEventType.Unknown,

                Code = "NEW_SHORTS",

                Description = "Price falling while Open Interest is increasing.",

                Strength = MarketStrength.Strong,

                Value = state.OpenInterest.Change.Close
            });
        }

        return events;
    }
}