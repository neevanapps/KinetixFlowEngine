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

            //------------------------------------------------
            // Stabilize very small flow values
            //------------------------------------------------

            const double minFlow = 0.01;

            if (Math.Abs(netFlow) < minFlow)
            {
                netFlow = Math.Sign(netFlow) == 0
                    ? minFlow
                    : Math.Sign(netFlow) * minFlow;
            }

            //------------------------------------------------
            // Efficiency calculation
            //------------------------------------------------

            double efficiency = priceMove / netFlow;

            //------------------------------------------------
            // ATR normalization
            //------------------------------------------------

            if (atr > 0)
                efficiency /= atr;

            //------------------------------------------------
            // Price control detection
            //------------------------------------------------

            bool bullishControl = false;
            bool bearishControl = false;

            if (priceMove > 0 && netFlow <= 0)
                bullishControl = true;

            if (priceMove < 0 && netFlow >= 0)
                bearishControl = true;

            return new FlowImpactSnapshot
            {
                Efficiency = efficiency,
                BullishControl = bullishControl,
                BearishControl = bearishControl
            };
        }
    }
}