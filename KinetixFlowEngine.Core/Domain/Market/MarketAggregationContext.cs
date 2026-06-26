using System.Collections.ObjectModel;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketAggregationContext
{
    private readonly IReadOnlyList<MarketState> _states;

    public MarketAggregationContext(
        IReadOnlyList<MarketState> states,
        MarketTimeframe timeframe)
    {
        if (states == null)
            throw new ArgumentNullException(nameof(states));

        if (states.Count == 0)
            throw new ArgumentException(
                "Aggregation requires at least one MarketState.",
                nameof(states));

        _states = states.ToArray();
        Timeframe = timeframe;
    }

    /// <summary>
    /// States in chronological order.
    /// </summary>
    public IReadOnlyList<MarketState> States => _states;

    /// <summary>
    /// First state in the aggregation window.
    /// </summary>
    public MarketState First => _states[0];

    /// <summary>
    /// Latest state in the aggregation window.
    /// </summary>
    public MarketState Last => _states[^1];

    /// <summary>
    /// Number of MarketStates.
    /// </summary>
    public int Count => _states.Count;

    /// <summary>
    /// Target timeframe.
    /// </summary>
    public MarketTimeframe Timeframe { get; }

    /// <summary>
    /// Aggregation start.
    /// </summary>
    public DateTime StartUtc => First.TimestampUtc;

    /// <summary>
    /// Aggregation end.
    /// </summary>
    public DateTime EndUtc => Last.TimestampUtc;

    /// <summary>
    /// Duration represented by this aggregation.
    /// </summary>
    public TimeSpan Duration => EndUtc - StartUtc;
}