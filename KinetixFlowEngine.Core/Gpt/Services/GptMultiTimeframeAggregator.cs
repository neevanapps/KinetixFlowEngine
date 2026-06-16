using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptMultiTimeframeAggregator
{
    private readonly GptMarketStateManager _marketStateManager;

    public GptMultiTimeframeAggregator(
        GptMarketStateManager marketStateManager)
    {
        _marketStateManager = marketStateManager;
    }

    public GptMultiTimeframeSnapshot Build()
    {
        var rows = _marketStateManager.Rows;

        return new GptMultiTimeframeSnapshot
        {
            ScoreZ = BuildArray(rows, x => x.ScoreZ),

            VelocityZ = BuildArray(rows, x => x.VelocityZ),

            ImbalanceZ = BuildArray(rows, x => x.ImbalanceZ),

            CompressionZ = BuildArray(rows, x => x.CompressionZ),

            ExhaustionZ = BuildArray(rows, x => x.ExhaustionZ),

            Momentum = BuildArray(rows, x => x.Momentum),

            Acceleration = BuildArray(rows, x => x.Acceleration),

            Persistence = BuildArray(rows, x => x.Persistence),

            NetPressure = BuildArray(rows, x => x.NetPressure),

            OIChange = BuildArray(rows, x => x.OIChange),

            FundingPressure = BuildArray(rows, x => x.FundingPressure),

            FlowImpactEfficiency =
                BuildArray(rows, x => x.FlowImpactEfficiency),

            ER5 = BuildArray(rows, x => x.ER5),

            ER30 = BuildArray(rows, x => x.ER30)
        };
    }

    private static double[] BuildArray(
        IReadOnlyList<GptMarketStateRow> rows,
        Func<GptMarketStateRow, double> selector)
    {
        return
        [
            Average(rows, 10, selector),
            Average(rows, 30, selector),
            Average(rows, 60, selector)
        ];
    }

    private static double Average(
        IReadOnlyList<GptMarketStateRow> rows,
        int minutes,
        Func<GptMarketStateRow, double> selector)
    {
        if (rows.Count == 0)
            return 0;

        var values =
            rows.TakeLast(
                Math.Min(minutes, rows.Count));

        return values.Average(selector);
    }
}