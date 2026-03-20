using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Prop
{
    public interface IPositionSizer
    {
        decimal CalculateSize(
            AccountRuntime account,
            decimal entryPrice,
            decimal stopLoss);
    }
    public class PropPositionSizer : IPositionSizer
    {
        private const decimal MIN_SIZE = 0.001m; // adjust for BTC

        public decimal CalculateSize(
            AccountRuntime acc,
            decimal entryPrice,
            decimal stopLoss)
        {
            var equity = acc.State.CurrentEquity;

            // -------------------------------
            // 1. RISK PER TRADE (CONFIGURABLE)
            // -------------------------------
            decimal riskPercent = 0.01m; // 1%
            decimal riskAmount = equity * riskPercent;

            // -------------------------------
            // 2. STOP DISTANCE
            // -------------------------------
            decimal stopDistance = Math.Abs(entryPrice - stopLoss);

            if (stopDistance <= 0)
                return 0;

            // -------------------------------
            // 3. RAW SIZE
            // -------------------------------
            decimal size = riskAmount / stopDistance;

            // -------------------------------
            // 4. LEVERAGE CAP
            // -------------------------------
            decimal maxNotional = equity * acc.Config.LeverageCap;

            decimal notional = size * entryPrice;

            if (notional > maxNotional)
            {
                size = maxNotional / entryPrice;
            }

            // -------------------------------
            // 5. MIN SIZE FILTER
            // -------------------------------
            if (size < MIN_SIZE)
                return 0;

            // -------------------------------
            // 6. ROUNDING (IMPORTANT)
            // -------------------------------
            size = Math.Round(size, 4); // Binance/Bybit precision

            return size;
        }
    }
}
