using KinetixFlowEngine.Core.Flow;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowImpactSnapshot
    {
        public double Efficiency { get; set; }

        public bool BullishControl { get; set; }
        public bool BearishControl { get; set; }
    }

    public class FlowImpactEngine
    {
        public FlowImpactSnapshot Calculate(
            double price,
            double previousPrice,
            FlowWindowSnapshot window,
            double atr)
        {
            double priceMove = price - previousPrice;

            double buyVol = (double)window.BuyVolume;
            double sellVol = (double)window.SellVolume;

            double netFlow = buyVol - sellVol;

            if (Math.Abs(netFlow) < 0.0001)
                netFlow = 0.0001;

            double efficiency = priceMove / netFlow;

            bool bullishControl = false;
            bool bearishControl = false;

            if (priceMove > 0 && netFlow <= 0)
                bullishControl = true;

            if (priceMove < 0 && netFlow >= 0)
                bearishControl = true;

            if (atr > 0)
                efficiency /= atr;

            return new FlowImpactSnapshot
            {
                Efficiency = efficiency,
                BullishControl = bullishControl,
                BearishControl = bearishControl
            };
        }
    }
}