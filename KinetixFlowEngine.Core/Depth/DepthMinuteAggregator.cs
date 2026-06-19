using System.Collections.Generic;

namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMinuteAggregator
{
    private readonly Queue<DepthSecondFeature> _samples = new();

    private const int MaxSamples = 120;

    public int Count => _samples.Count;

    public void Add(
        DepthSecondFeature feature)
    {
        _samples.Enqueue(feature);

        while (_samples.Count > MaxSamples)
        {
            _samples.Dequeue();
        }
    }

    public bool IsReady()
    {
        return _samples.Count >= MaxSamples;
    }

    public DepthMinuteSummary Build()
    {
        var samples = _samples.ToArray();

        var bullish =
            samples.Count(x => x.ImbalanceTop10 > 0);

        var bearish =
            samples.Count(x => x.ImbalanceTop10 < 0);

        var first = samples.First();
        var last = samples.Last();

        var bullishPersistence =
    GetLongestRun(
        samples,
        x => x.ImbalanceTop10 > 0);

        var bearishPersistence =
            GetLongestRun(
                samples,
                x => x.ImbalanceTop10 < 0);

        return new DepthMinuteSummary
        {
            TimestampUtc = DateTime.UtcNow,

            AverageImbalanceTop5 =
        samples.Average(x => x.ImbalanceTop5),

            AverageImbalanceTop10 =
        samples.Average(x => x.ImbalanceTop10),

            MaxBullishImbalance =
        samples.Max(x => x.ImbalanceTop10),

            MaxBearishImbalance =
        samples.Min(x => x.ImbalanceTop10),

            BullishBookPercent =
        samples.Length == 0
            ? 0
            : bullish * 100.0 / samples.Length,

            BearishBookPercent =
        samples.Length == 0
            ? 0
            : bearish * 100.0 / samples.Length,

            PriceChange1m =
        last.Price - first.Price,

            BullishPersistenceSeconds =
        bullishPersistence,

            BearishPersistenceSeconds =
        bearishPersistence,

            SampleCount =
        samples.Length
        };
    }

    private static int GetLongestRun(
    IEnumerable<DepthSecondFeature> samples,
    Func<DepthSecondFeature, bool> predicate)
    {
        var longest = 0;
        var current = 0;

        foreach (var sample in samples)
        {
            if (predicate(sample))
            {
                current++;

                if (current > longest)
                {
                    longest = current;
                }
            }
            else
            {
                current = 0;
            }
        }

        return longest;
    }
}