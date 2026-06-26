using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketConsensusBuilder
{
    public ConsensusResult Build(MarketState state)
    {
        var supporting = new List<string>();
        var opposing = new List<string>();

        decimal bullishScore = 0;
        decimal bearishScore = 0;
        decimal neutralScore = 0;

        int bullishVotes = 0;
        int bearishVotes = 0;
        int neutralVotes = 0;

        AddVote(
            "Price",
            state.Price.Summary,
            supporting,
            opposing,
            ref bullishVotes,
            ref bearishVotes,
            ref neutralVotes,
            ref bullishScore,
            ref bearishScore,
            ref neutralScore);

        AddVote(
            "Trade",
            state.Trade.Summary,
            supporting,
            opposing,
            ref bullishVotes,
            ref bearishVotes,
            ref neutralVotes,
            ref bullishScore,
            ref bearishScore,
            ref neutralScore);

        AddVote(
            "Depth",
            state.Depth.Summary,
            supporting,
            opposing,
            ref bullishVotes,
            ref bearishVotes,
            ref neutralVotes,
            ref bullishScore,
            ref bearishScore,
            ref neutralScore);

        AddVote(
            "Funding",
            state.Funding.Summary,
            supporting,
            opposing,
            ref bullishVotes,
            ref bearishVotes,
            ref neutralVotes,
            ref bullishScore,
            ref bearishScore,
            ref neutralScore);

        AddVote(
            "OpenInterest",
            state.OpenInterest.Summary,
            supporting,
            opposing,
            ref bullishVotes,
            ref bearishVotes,
            ref neutralVotes,
            ref bullishScore,
            ref bearishScore,
            ref neutralScore);

        var bias = MarketBias.Neutral;
        var dominantScore = neutralScore;

        if (bullishScore > dominantScore)
        {
            bias = MarketBias.Bullish;
            dominantScore = bullishScore;
        }

        if (bearishScore > dominantScore)
        {
            bias = MarketBias.Bearish;
            dominantScore = bearishScore;
        }

        var totalScore =
            bullishScore +
            bearishScore +
            neutralScore;

        byte confidence = 0;

        if (totalScore > 0)
        {
            confidence = (byte)Math.Round(
                dominantScore / totalScore * 100m);
        }

        return new ConsensusResult
        {
            Bias = bias,

            Strength = CalculateStrength(confidence),

            Confidence = confidence,

            BullishVotes = bullishVotes,

            BearishVotes = bearishVotes,

            NeutralVotes = neutralVotes,

            BullishScore = bullishScore,

            BearishScore = bearishScore,

            NeutralScore = neutralScore,

            HasConflict =
                bullishVotes > 0 &&
                bearishVotes > 0,

            SupportingDomains = supporting,

            OpposingDomains = opposing
        };
    }

    private static void AddVote(
        string name,
        DomainSummary summary,
        List<string> supporting,
        List<string> opposing,
        ref int bullishVotes,
        ref int bearishVotes,
        ref int neutralVotes,
        ref decimal bullishScore,
        ref decimal bearishScore,
        ref decimal neutralScore)
    {
        decimal weight = GetWeight(summary.Strength);

        switch (summary.Bias)
        {
            case MarketBias.Bullish:

                bullishVotes++;

                bullishScore += weight;

                supporting.Add(name);

                break;

            case MarketBias.Bearish:

                bearishVotes++;

                bearishScore += weight;

                opposing.Add(name);

                break;

            default:

                neutralVotes++;

                neutralScore += weight;

                break;
        }
    }

    private static decimal GetWeight(
        MarketStrength strength)
    {
        return strength switch
        {
            MarketStrength.Extreme => 4m,
            MarketStrength.Strong => 3m,
            MarketStrength.Moderate => 2m,
            _ => 1m
        };
    }

    private static MarketStrength CalculateStrength(
        byte confidence)
    {
        if (confidence >= 90)
            return MarketStrength.Extreme;

        if (confidence >= 75)
            return MarketStrength.Strong;

        if (confidence >= 60)
            return MarketStrength.Moderate;

        return MarketStrength.Weak;
    }
}