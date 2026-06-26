using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.OI;

public sealed class OIEventBuilder
{
    public IReadOnlyList<MarketEvent> Build(OpenInterest oi)
    {
        var events = new List<MarketEvent>();

        if (oi.Change.Close >= 100_000_000m)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = DateTime.UtcNow,

                Type = MarketEventType.OpenInterestIncreasing,

                Strength = MarketStrength.Strong,

                Value = oi.Change.Close,

                Code = "OI_UP",

                Description = "Open interest increased significantly."
            });
        }

        if (oi.Change.Close <= -100_000_000m)
        {
            events.Add(new MarketEvent
            {
                TimestampUtc = DateTime.UtcNow,

                Type = MarketEventType.OpenInterestDecreasing,

                Strength = MarketStrength.Strong,

                Value = Math.Abs(oi.Change.Close),

                Code = "OI_DOWN",

                Description = "Open interest decreased significantly."
            });
        }

        return events;
    }
}