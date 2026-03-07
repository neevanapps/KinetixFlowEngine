namespace KinetixFlowEngine.Core.Utils
{
    public class FairPriceEngine
    {
        public bool IsFairLongEntry(decimal price, double vwap, double atr)
        {
            if (atr <= 0)
                return true;

            var deviation = price - (decimal)vwap;

            var deviationAtr = deviation / (decimal)atr;

            return deviationAtr <= -0.3m;
        }

        public bool IsFairShortEntry(decimal price, double vwap, double atr)
        {
            if (atr <= 0)
                return true;

            var deviation = price - (decimal)vwap;

            var deviationAtr = deviation / (decimal)atr;

            return deviationAtr >= 0.3m;
        }

        public decimal GetDeviationAtr(decimal price, double vwap, double atr)
        {
            if (atr <= 0)
                return 0;

            var deviation = price - (decimal)vwap;

            return deviation / (decimal)atr;
        }
    }
}