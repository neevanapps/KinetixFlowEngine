namespace KinetixFlowEngine.Core.Domain.Trading;

public sealed class TradeBuilder
{
    public Trade Build(TradeMinuteSnapshot snapshot)
    {
        return new Trade
        {
            Volume = BuildVolume(snapshot),

            Trades = BuildTradeCount(snapshot),

            TradeSize = BuildTradeSize(snapshot),

            Execution = BuildExecution(snapshot)
        };
    }

    private static TradeVolume BuildVolume(
        TradeMinuteSnapshot snapshot)
    {
        return new TradeVolume
        {
            Buy = MetricStateFactory.Create(snapshot.BuyVolume),

            Sell = MetricStateFactory.Create(snapshot.SellVolume),

            Delta = MetricStateFactory.Create(snapshot.BuyVolume - snapshot.SellVolume)
        };
    }

    private static TradeCount BuildTradeCount(
        TradeMinuteSnapshot snapshot)
    {
        return new TradeCount
        {
            Buy = MetricStateFactory.Create(snapshot.BuyTrades),

            Sell = MetricStateFactory.Create(snapshot.SellTrades)
        };
    }

    private static TradeSize BuildTradeSize(
        TradeMinuteSnapshot snapshot)
    {
        return new TradeSize
        {
            Buy = MetricStateFactory.Create(snapshot.AverageBuyTradeSize),

            Sell = MetricStateFactory.Create(snapshot.AverageSellTradeSize),

            LargestBuy = MetricStateFactory.Create(snapshot.LargestBuyTrade),

            LargestSell = MetricStateFactory.Create(snapshot.LargestSellTrade)
        };
    }

    private static TradeExecution BuildExecution(
        TradeMinuteSnapshot snapshot)
    {
        return new TradeExecution
        {
            BuyVWAP = MetricStateFactory.Create(snapshot.BuyVWAP),

            SellVWAP = MetricStateFactory.Create(snapshot.SellVWAP)
        };
    }

}