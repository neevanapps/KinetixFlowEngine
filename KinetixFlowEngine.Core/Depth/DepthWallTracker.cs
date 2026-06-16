using KinetixFlowEngine.Core.Data;

namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthWallTracker
{
    private readonly Dictionary<decimal, DepthWall> _bidWalls = new();

    private readonly Dictionary<decimal, DepthWall> _askWalls = new();

    private const decimal MinWallSize = 10m;

    public void Update(
        DepthSnapshot snapshot)
    {
        UpdateSide(
            snapshot.Bids,
            _bidWalls);

        UpdateSide(
            snapshot.Asks,
            _askWalls);

        Cleanup();
    }

    private static void UpdateSide(
        IReadOnlyList<DepthLevel> levels,
        Dictionary<decimal, DepthWall> walls)
    {
        var now = DateTime.UtcNow;

        foreach (var level in levels)
        {
            if (level.Quantity < MinWallSize)
                continue;

            if (!walls.TryGetValue(
                    level.Price,
                    out var wall))
            {
                wall = new DepthWall
                {
                    Price = level.Price,
                    InitialQuantity = level.Quantity,
                    CurrentQuantity = level.Quantity,
                    FirstSeenUtc = now,
                    LastSeenUtc = now,
                    MaxQuantitySeen = level.Quantity
                };

                walls[level.Price] = wall;
            }
            else
            {
                if (level.Quantity < wall.CurrentQuantity * 0.95m)
                {
                    wall.IsConsumed = true;
                }

                wall.CurrentQuantity =
                    level.Quantity;

                wall.QuantityChangePercent =
    wall.InitialQuantity <= 0
        ? 0
        : (double)(
            (wall.CurrentQuantity -
             wall.InitialQuantity)
             / wall.InitialQuantity * 100m);

                wall.MaxQuantitySeen =
    Math.Max(
        wall.MaxQuantitySeen,
        level.Quantity);

                wall.LastSeenUtc = now;
            }


        }
    }

    private void Cleanup()
    {
        var cutoff =
            DateTime.UtcNow.AddSeconds(-90);

        RemoveOldWalls(
            _bidWalls,
            cutoff);

        RemoveOldWalls(
            _askWalls,
            cutoff);
    }

    private static void RemoveOldWalls(
        Dictionary<decimal, DepthWall> walls,
        DateTime cutoff)
    {
        var expired =
            walls
                .Where(x =>
                    x.Value.LastSeenUtc < cutoff)
                .Select(x => x.Key)
                .ToList();

        foreach (var key in expired)
        {
            walls.Remove(key);
        }
    }

    public IReadOnlyList<DepthWall> GetTopBidWalls()
    {
        return _bidWalls.Values
            .Where(x =>
                DurationSeconds(x) >= 5)
            .OrderByDescending(x =>
    DurationSeconds(x))
.ThenByDescending(x =>
    x.MaxQuantitySeen)
.Take(5)
            .ToList();
    }

    public IReadOnlyList<DepthWall> GetTopAskWalls()
    {
        return _askWalls.Values
            .Where(x =>
                DurationSeconds(x) >= 5)
            .OrderByDescending(x =>
    DurationSeconds(x))
.ThenByDescending(x =>
    x.MaxQuantitySeen)
.Take(5)
            .ToList();
    }

    private static double DurationSeconds(
        DepthWall wall)
    {
        return
            (wall.LastSeenUtc -
             wall.FirstSeenUtc)
            .TotalSeconds;
    }
}