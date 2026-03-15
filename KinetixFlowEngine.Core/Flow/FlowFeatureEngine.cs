using KinetixFlowEngine.Core.Flow.Features;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowFeatureEngine
    {
        private readonly FlowMomentum _momentum = new();
        private readonly FlowAcceleration _acceleration = new();
        private readonly FlowPersistence _persistence = new();
        private readonly DeltaVelocity _deltaVelocity = new();
        private readonly FlowCompressionEngine _compression = new();
        private readonly LiquidityExhaustionEngine _exhaustion = new();

        public FlowFeatureSnapshot Calculate(FlowWindowSnapshot window, double price, double previousPrice, double atr)
        {
            var imbalance = FlowImbalance.Calculate(window.BuyVolume, window.SellVolume);

            var compression = _compression.Update(imbalance);

            var deltaVelocity = _deltaVelocity.Calculate(imbalance);

            var momentum = _momentum.Calculate(imbalance);

            var acceleration = _acceleration.Calculate(momentum);

            var persistence = _persistence.Calculate(imbalance);

            var exhaustion = _exhaustion.Update(price, deltaVelocity);

            var sizeBias = TradeSizeBias.Calculate(
                window.BuyVolume,
                window.SellVolume,
                window.BuyTrades,
                window.SellTrades);

            var absorption = AbsorptionDetector.Detect(
                window.BuyVolume,
                window.SellVolume,
                price,
                previousPrice,
                atr);

            return new FlowFeatureSnapshot
            {
                Imbalance = imbalance,
                Momentum = momentum,
                Acceleration = acceleration,
                Persistence = persistence,
                SizeBias = sizeBias,
                Absorption = absorption,
                DeltaVelocity = deltaVelocity,
                Compression = compression,
                Exhaustion = exhaustion
            };
        }
    }
}