using KinetixFlowEngine.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.FundingRate
{
    public sealed class FundingEventBuilder
    {
        public IReadOnlyList<MarketEvent> Build(Funding funding)
        {
            var events = new List<MarketEvent>();

            if (funding.Rate.Close >= 0.05m)
            {
                events.Add(new MarketEvent
                {
                    TimestampUtc = DateTime.UtcNow,

                    Type = MarketEventType.FundingSpike,

                    Strength = MarketStrength.Strong,

                    Value = funding.Rate.Close,

                    Code = "FUNDING_SPIKE",

                    Description = "Funding rate increased sharply."
                });
            }

            if (funding.Rate.Close <= -0.05m)
            {
                events.Add(new MarketEvent
                {
                    TimestampUtc = DateTime.UtcNow,

                    Type = MarketEventType.FundingDrop,

                    Strength = MarketStrength.Strong,

                    Value = Math.Abs(funding.Rate.Close),

                    Code = "FUNDING_DROP",

                    Description = "Funding rate decreased sharply."
                });
            }

            return events;
        }
    }
}
