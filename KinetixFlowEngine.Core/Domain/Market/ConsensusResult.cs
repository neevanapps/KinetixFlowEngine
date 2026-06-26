using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class ConsensusResult
{
    //----------------------------------------
    // Overall
    //----------------------------------------

    public MarketBias Bias { get; init; }

    public MarketStrength Strength { get; init; }

    /// <summary>
    /// 0-100 confidence.
    /// </summary>
    public byte Confidence { get; init; }

    //----------------------------------------
    // Voting
    //----------------------------------------

    public int BullishVotes { get; init; }

    public int BearishVotes { get; init; }

    public int NeutralVotes { get; init; }

    //----------------------------------------
    // Weighted scores
    //----------------------------------------

    public decimal BullishScore { get; init; }

    public decimal BearishScore { get; init; }

    public decimal NeutralScore { get; init; }

    //----------------------------------------
    // Diagnostics
    //----------------------------------------

    public bool HasConflict { get; init; }

    public IReadOnlyList<string> SupportingDomains { get; init; }
        = Array.Empty<string>();

    public IReadOnlyList<string> OpposingDomains { get; init; }
        = Array.Empty<string>();
}