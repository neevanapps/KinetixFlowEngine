using KinetixFlowEngine.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.FundingRate
{
    public sealed class FundingSummaryBuilder
    {
        public DomainSummary Build(Funding funding)
        {
            var bias = MarketBias.Neutral;

            if (funding.Rate.Close > 0)
                bias = MarketBias.Bullish;

            if (funding.Rate.Close < 0)
                bias = MarketBias.Bearish;

            var strength = MarketStrength.Weak;

            var absRate = Math.Abs(funding.Rate.Close);

            if (absRate >= 0.05m)
                strength = MarketStrength.Extreme;
            else if (absRate >= 0.02m)
                strength = MarketStrength.Strong;
            else if (absRate >= 0.01m)
                strength = MarketStrength.Moderate;

            return new DomainSummary
            {
                Bias = bias,

                Strength = strength,

                Narrative =
                    $"Funding rate is {funding.Rate.Close:F4}% with funding pressure at {funding.Pressure.Close:F2}."
            };
        }
    }
}
