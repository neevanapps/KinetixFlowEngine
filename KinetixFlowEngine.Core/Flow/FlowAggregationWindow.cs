using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Models;

public class FlowAggregationWindow
{
    private readonly Queue<FlowTrade> _window = new();

    private readonly int _windowSeconds;

    private decimal _buyVolume;
    private decimal _sellVolume;

    private int _buyTrades;
    private int _sellTrades;

    public FlowAggregationWindow(int windowSeconds = 60)
    {
        _windowSeconds = windowSeconds;
    }

    public void AddTrade(FlowTrade trade)
    {
        _window.Enqueue(trade);

        if (!trade.IsBuyerMaker)
        {
            _buyVolume += trade.Quantity;
            _buyTrades++;
        }
        else
        {
            _sellVolume += trade.Quantity;
            _sellTrades++;
        }

        Cleanup(trade.Timestamp);
    }

    private void Cleanup(long currentTimestamp)
    {
        long cutoff = currentTimestamp - (_windowSeconds * 1000);

        while (_window.Count > 0)
        {
            var t = _window.Peek();

            if (t.Timestamp >= cutoff)
                break;

            _window.Dequeue();

            if (!t.IsBuyerMaker)
            {
                _buyVolume -= t.Quantity;
                _buyTrades--;
            }
            else
            {
                _sellVolume -= t.Quantity;
                _sellTrades--;
            }
        }
    }

    public FlowWindowSnapshot GetSnapshot()
    {
        return new FlowWindowSnapshot
        {
            BuyVolume = _buyVolume,
            SellVolume = _sellVolume,
            BuyTrades = _buyTrades,
            SellTrades = _sellTrades
        };
    }
}