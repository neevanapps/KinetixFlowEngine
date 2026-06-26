namespace KinetixFlowEngine.Core.Database;

public sealed class MarketMinuteFeatureEntity
{
    //--------------------------------------------------
    // Identity
    //--------------------------------------------------

    public long Id { get; set; }

    public int Sequence { get; set; }

    public DateTime TimestampUtc { get; set; }

    //--------------------------------------------------
    // Trading Session
    //--------------------------------------------------

    public string Session { get; set; } = string.Empty;

    //--------------------------------------------------
    // Price
    //--------------------------------------------------

    public decimal Open { get; set; }

    public decimal High { get; set; }

    public decimal Low { get; set; }

    public decimal Close { get; set; }

    public decimal VWAP { get; set; }

    //--------------------------------------------------
    // Price Behaviour
    //--------------------------------------------------

    public decimal PriceChange { get; set; }

    public decimal PriceChangePct { get; set; }

    public decimal Range { get; set; }

    public decimal RangePct { get; set; }

    public decimal ATR { get; set; }

    public decimal DistanceFromVWAP { get; set; }

    public decimal DistanceFromVWAPPct { get; set; }

    //--------------------------------------------------
    // Candle Structure
    //--------------------------------------------------

    public decimal BodyPct { get; set; }

    public decimal UpperWickPct { get; set; }

    public decimal LowerWickPct { get; set; }

    //--------------------------------------------------
    // Volume
    //--------------------------------------------------

    public decimal BuyVolume { get; set; }

    public decimal SellVolume { get; set; }

    public decimal TotalVolume { get; set; }

    public decimal BuyVolumePct { get; set; }

    public decimal SellVolumePct { get; set; }

    //--------------------------------------------------
    // Trades
    //--------------------------------------------------

    public int BuyTrades { get; set; }

    public int SellTrades { get; set; }

    public int TotalTrades { get; set; }

    public decimal AverageBuySize { get; set; }

    public decimal AverageSellSize { get; set; }

    public decimal LargestBuyTrade { get; set; }

    public decimal LargestSellTrade { get; set; }

    public decimal BuyVWAP { get; set; }

    public decimal SellVWAP { get; set; }

    //--------------------------------------------------
    // Funding / Open Interest
    //--------------------------------------------------

    public double FundingRate { get; set; }

    public double FundingPressure { get; set; }

    public double OIChange { get; set; }

    //--------------------------------------------------
    // Order Book Depth
    //--------------------------------------------------

    public double DepthImbalance { get; set; }

    public double DepthBullPct { get; set; }

    public double BidWallAge { get; set; }

    public double AskWallAge { get; set; }

    public decimal BidWallQty { get; set; }

    public decimal AskWallQty { get; set; }

    public double BullishPersistence { get; set; }

    public double BidConsumption { get; set; }

    public double AskConsumption { get; set; }
}