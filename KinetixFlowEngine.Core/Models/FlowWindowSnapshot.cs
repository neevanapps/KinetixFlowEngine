namespace KinetixFlowEngine.Core.Flow;

public sealed class FlowWindowSnapshot
{
    //----------------------------------------
    // Volume
    //----------------------------------------

    public decimal BuyVolume { get; init; }

    public decimal SellVolume { get; init; }

    public decimal TotalVolume => BuyVolume + SellVolume;

    public decimal BuyVolumePct =>
        TotalVolume == 0
            ? 0
            : BuyVolume / TotalVolume * 100m;

    public decimal SellVolumePct =>
        TotalVolume == 0
            ? 0
            : SellVolume / TotalVolume * 100m;

    public decimal DeltaVolume =>
        BuyVolume - SellVolume;

    public decimal DeltaPct =>
        BuyVolumePct - SellVolumePct;

    //----------------------------------------
    // Trades
    //----------------------------------------

    public int BuyTrades { get; init; }

    public int SellTrades { get; init; }

    public int TotalTrades =>
        BuyTrades + SellTrades;

    public decimal BuyTradePct =>
        TotalTrades == 0
            ? 0
            : (decimal)BuyTrades / TotalTrades * 100m;

    public decimal SellTradePct =>
        TotalTrades == 0
            ? 0
            : (decimal)SellTrades / TotalTrades * 100m;

    //----------------------------------------
    // Average Trade Size
    //----------------------------------------

    public decimal AverageBuySize { get; init; }

    public decimal AverageSellSize { get; init; }

    //----------------------------------------
    // Largest Trades
    //----------------------------------------

    public decimal LargestBuyTrade { get; init; }

    public decimal LargestSellTrade { get; init; }

    //----------------------------------------
    // Execution VWAP
    //----------------------------------------

    public decimal BuyVWAP { get; init; }

    public decimal SellVWAP { get; init; }

    //----------------------------------------
    // Convenience Metrics
    //----------------------------------------

    public decimal VWAPDifference =>
        BuyVWAP - SellVWAP;

    public decimal AverageTradeSizeDifference =>
        AverageBuySize - AverageSellSize;

    public decimal LargestTradeDifference =>
        LargestBuyTrade - LargestSellTrade;

    //----------------------------------------
    // Simple State
    //----------------------------------------

    public bool BuyDominant =>
        BuyVolume > SellVolume;

    public bool SellDominant =>
        SellVolume > BuyVolume;
}