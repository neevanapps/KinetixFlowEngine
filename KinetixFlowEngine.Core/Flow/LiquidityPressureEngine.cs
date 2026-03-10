using KinetixFlowEngine.Core.Flow;

namespace KinetixFlowEngine.Core.Flow
{
    public class LiquidityPressureSnapshot
    {
        public double BuyPressure { get; set; }
        public double SellPressure { get; set; }
        public double NetPressure => BuyPressure - SellPressure;

        public bool BullishBreakout { get; set; }
        public bool BearishBreakout { get; set; }
    }

    public class LiquidityPressureEngine
    {
        public LiquidityPressureSnapshot Calculate(
            FlowWindowSnapshot window,
            double price,
            double atr,
            double vwap)
        {
            double buyVolume = (double)window.BuyVolume;
            double sellVolume = (double)window.SellVolume;

            double flow = buyVolume - sellVolume;

            double resistance = Math.Abs(price - vwap) + atr;

            if (resistance <= 0)
                resistance = 1;

            double pressure = flow / resistance;

            bool bullishBreakout = pressure > 2;
            bool bearishBreakout = pressure < -2;

            return new LiquidityPressureSnapshot
            {
                BuyPressure = Math.Max(pressure, 0),
                SellPressure = Math.Max(-pressure, 0),
                BullishBreakout = bullishBreakout,
                BearishBreakout = bearishBreakout
            };
        }
    }
}