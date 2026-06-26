using KinetixFlowEngine.Core.Domain.Liquidity;
using KinetixFlowEngine.Core.Flow.Builders;

namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMinuteBuilder : MinuteStateBuilderBase
{
    //public DepthMinuteState Build(
    //    DepthMinuteSnapshot buffer,
    //    DepthWallState wallState)
    //{
    //    foreach (var snapshot in buffer.Snapshots)
    //    {
    //        decimal bidLiquidity =
    //            snapshot.Bids.Sum(x => x.Quantity);

    //        decimal askLiquidity =
    //            snapshot.Asks.Sum(x => x.Quantity);

    //        decimal imbalance =
    //            DepthMath.CalculateImbalance(
    //                bidLiquidity,
    //                askLiquidity);

    //        Builder1.Add(bidLiquidity);

    //        Builder2.Add(askLiquidity);

    //        Builder3.Add(imbalance);
    //    }

    //    var bidState = Builder1.Build();

    //    var askState = Builder2.Build();

    //    var imbalanceState = Builder3.Build();

    //    var result = new DepthMinuteState
    //    {
    //        MinuteUtc = buffer.MinuteUtc,

    //        BidLiquidity = bidState,

    //        AskLiquidity = askState,

    //        Imbalance = imbalanceState,

    //        BidLiquidityBehaviour =
    //            MetricBehaviourEvaluator.EvaluateLiquidity(
    //                bidState),

    //        AskLiquidityBehaviour =
    //            MetricBehaviourEvaluator.EvaluateLiquidity(
    //                askState),

    //        ImbalanceBehaviour =
    //            MetricBehaviourEvaluator.EvaluateImbalance(
    //                imbalanceState),

    //        Walls = wallState
    //    };

    //    Reset();

    //    return result;
    //}
}