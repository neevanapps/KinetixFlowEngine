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
            //------------------------------------------------
            // Safety checks
            //------------------------------------------------

            if (previousPrice <= 0 || atr <= 0)
            {
                return new FlowImpactSnapshot
                {
                    Efficiency = 0,
                    BullishControl = false,
                    BearishControl = false
                };
            }

            //------------------------------------------------
            // Price movement normalized by ATR
            //------------------------------------------------

            double priceMove = price - previousPrice;

            double normalizedMove =
                priceMove / atr;

            //------------------------------------------------
            // Flow imbalance
            //------------------------------------------------

            double buyVol = (double)window.BuyVolume;
            double sellVol = (double)window.SellVolume;

            double totalFlow = buyVol + sellVol;

            double imbalance = 0;

            if (totalFlow > 0)
            {
                imbalance =
                    (buyVol - sellVol) /
                    totalFlow;
            }

            //------------------------------------------------
            // Flow Impact Efficiency
            //
            // Interpretation:
            //
            // +1  = aggressive buying efficiently moves price
            //  0  = neutral
            // -1  = aggressive flow absorbed / ineffective
            //
            //------------------------------------------------

            double rawEfficiency =
                normalizedMove * imbalance;

            //------------------------------------------------
            // Compress to bounded range
            //------------------------------------------------

            double efficiency =
                Math.Tanh(rawEfficiency * 10.0);

            //------------------------------------------------
            // Price control detection
            //------------------------------------------------

            bool bullishControl = false;
            bool bearishControl = false;

            if (priceMove > 0 && imbalance <= 0)
                bullishControl = true;

            if (priceMove < 0 && imbalance >= 0)
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