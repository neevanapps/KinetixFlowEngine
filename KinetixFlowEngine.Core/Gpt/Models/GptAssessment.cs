namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptAssessment
{
    public DirectionalBias DirectionalBias { get; init; }

    public int LongConfidence { get; init; }

    public int ShortConfidence { get; init; }

    public decimal Score { get; init; }

    public int TrendQuality { get; init; }

    public int FlowQuality { get; init; }

    public int RegimeQuality { get; init; }

    public RiskLevel RiskLevel { get; init; }

    public StateAssessment StateAssessment { get; init; }

    public string Summary { get; init; } = string.Empty;

    public List<string> KeyDrivers { get; init; } = [];

    public List<string> Contradictions { get; init; } = [];

    public string DominantIntent { get; init; } = string.Empty;

    public List<string> BehaviorEvidence { get; init; } = [];

    public string MarketStructure { get; init; } = string.Empty;
}

public enum RiskLevel
{
    Low,
    Medium,
    High
}

public enum DirectionalBias
{
    Neutral = 0,
    Long = 1,
    Short = 2
}

public enum StateAssessment
{
    Accelerating = 0,
    Strengthening = 1,
    Ranging = 2,
    Exhausting = 3,
    Reversing = 4
}