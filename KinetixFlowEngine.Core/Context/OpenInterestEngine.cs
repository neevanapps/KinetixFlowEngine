using KinetixFlowEngine.Core.Utils;

public class OpenInterestEngine
{
    private readonly Ema _ema = new(10);
    private double _previous;

    public double Update(double current)
    {
        var change = current - _previous;
        _previous = current;

        return _ema.Update(change);
    }
}