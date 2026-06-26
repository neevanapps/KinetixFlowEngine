using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Pricing;

public sealed class PriceEventBuilder
{
    public IReadOnlyList<MarketEvent> Build(Price price)
    {
        var events = new List<MarketEvent>();

        if (price.Candle.Close > price.VWAP)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = price.Candle.MinuteUtc,

                Type = MarketEventType.VWAPReclaimed,

                Strength = MarketStrength.Moderate,

                Code = "VWAP_RECLAIMED",

                Description = "Price closed above VWAP."
            });
        }

        if (price.Candle.Close < price.VWAP)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = price.Candle.MinuteUtc,

                Type = MarketEventType.VWAPLost,

                Strength = MarketStrength.Moderate,

                Code = "VWAP_LOST",

                Description = "Price closed below VWAP."
            });
        }

        return events;
    }
}