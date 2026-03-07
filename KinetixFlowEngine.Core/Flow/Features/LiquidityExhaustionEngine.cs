namespace KinetixFlowEngine.Core.Flow.Features
{
    public class LiquidityExhaustionEngine
    {
        private double _prevPrice;

        public double Update(double price, double deltaVelocity)
        {
            if (_prevPrice == 0)
            {
                _prevPrice = price;
                return 0;
            }

            var priceMove = Math.Abs(price - _prevPrice);
            _prevPrice = price;

            if (priceMove < 0.01)
                priceMove = 0.01;

            var exhaustion = Math.Abs(deltaVelocity) / priceMove;

            return exhaustion;
        }
    }
}