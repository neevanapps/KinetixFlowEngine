namespace KinetixFlowEngine.Core.Context
{
    public class VwapEngine
    {
        private decimal _cumVolume;
        private decimal _cumPriceVolume;

        public decimal Update(decimal price, decimal volume)
        {
            _cumVolume += volume;
            _cumPriceVolume += price * volume;

            if (_cumVolume == 0)
                return price;

            return _cumPriceVolume / _cumVolume;
        }

        public double Deviation(decimal price, decimal vwap)
        {
            if (vwap == 0)
                return 0;

            return (double)((price - vwap) / vwap);
        }
    }
}