using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Trading;

public sealed class TradeEventBuilder
{
    public IReadOnlyList<MarketEvent> Build(Trade trade)
    {
        var events = new List<MarketEvent>();

        AddDeltaEvents(trade, events);

        AddLargeTradeEvents(trade, events);

        return events;
    }

    private static void AddDeltaEvents(
        Trade trade,
        List<MarketEvent> events)
    {
        if (trade.Volume.Delta.Close > 50m)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = DateTime.UtcNow,
                Type = MarketEventType.AggressiveBuying,
                Strength = MarketStrength.Strong,
                Value = trade.Volume.Delta.Close,
                Code = "BUY_DELTA",
                Description = "Buy volume significantly exceeded sell volume."
            });
        }

        if (trade.Volume.Delta.Close < -50m)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = DateTime.UtcNow,
                Type = MarketEventType.AggressiveSelling,
                Strength = MarketStrength.Strong,
                Value = Math.Abs(trade.Volume.Delta.Close),
                Code = "SELL_DELTA",
                Description = "Sell volume significantly exceeded buy volume."
            });
        }
    }

    private static void AddLargeTradeEvents(
        Trade trade,
        List<MarketEvent> events)
    {
        if (trade.TradeSize.LargestBuy.Close >
            trade.TradeSize.LargestSell.Close * 2)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = DateTime.UtcNow,
                Type = MarketEventType.LargeBuyerAppeared,
                Strength = MarketStrength.Moderate,
                Value = trade.TradeSize.LargestBuy.Close,
                Code = "LARGE_BUY",
                Description = "Large buy trade dominated the minute."
            });
        }

        if (trade.TradeSize.LargestSell.Close >
            trade.TradeSize.LargestBuy.Close * 2)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = DateTime.UtcNow,
                Type = MarketEventType.LargeSellerAppeared,
                Strength = MarketStrength.Moderate,
                Value = trade.TradeSize.LargestSell.Close,
                Code = "LARGE_SELL",
                Description = "Large sell trade dominated the minute."
            });
        }
    }
}