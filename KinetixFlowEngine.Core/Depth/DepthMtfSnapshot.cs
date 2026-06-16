namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMtfSnapshot
{
    public double[] Imbalance { get; set; } = [];

    public double[] BullishPercent { get; set; } = [];

    public double[] BullishPersistence { get; set; } = [];

    public double[] BidWallAge { get; set; } = [];

    public double[] AskWallAge { get; set; } = [];

    public double[] BidWallQty { get; set; } = [];

    public double[] AskWallQty { get; set; } = [];

    public double[] BidConsumption { get; set; } = [];

    public double[] AskConsumption { get; set; } = [];
}