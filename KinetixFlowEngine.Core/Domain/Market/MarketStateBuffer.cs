using System.Collections.ObjectModel;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketStateBuffer
{
    private readonly Queue<MarketState> _states = new();

    public int Capacity { get; }

    public MarketStateBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        Capacity = capacity;
    }

    /// <summary>
    /// Current states in chronological order (oldest -> newest).
    /// </summary>
    public IReadOnlyCollection<MarketState> States =>
        new ReadOnlyCollection<MarketState>(_states.ToList());

    /// <summary>
    /// True when the buffer contains the required number of states.
    /// </summary>
    public bool IsComplete =>
        _states.Count == Capacity;

    /// <summary>
    /// Current number of states.
    /// </summary>
    public int Count =>
        _states.Count;

    /// <summary>
    /// Oldest state.
    /// </summary>
    public MarketState? First =>
        _states.Count == 0
            ? null
            : _states.Peek();

    /// <summary>
    /// Latest state.
    /// </summary>
    public MarketState? Last =>
        _states.Count == 0
            ? null
            : _states.Last();

    /// <summary>
    /// Adds a new MarketState while maintaining the rolling window.
    /// </summary>
    public void Add(MarketState state)
    {
        _states.Enqueue(state);

        while (_states.Count > Capacity)
            _states.Dequeue();
    }

    /// <summary>
    /// Removes all buffered states.
    /// </summary>
    public void Clear()
    {
        _states.Clear();
    }
}