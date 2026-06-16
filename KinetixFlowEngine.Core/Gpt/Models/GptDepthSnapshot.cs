namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptDepthSnapshot
{
    public double[] DepthImbalance { get; init; } = [];

    public double[] DepthBullPct { get; init; } = [];

    public double[] BidWallAge { get; init; } = [];

    public double[] AskWallAge { get; init; } = [];

    public double[] BidWallQty { get; init; } = [];

    public double[] AskWallQty { get; init; } = [];
}