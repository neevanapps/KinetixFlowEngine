using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Models
{
    public sealed class CandleSnapshot
    {
        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public decimal Range =>
            High - Low;

        public decimal Body =>
            Math.Abs(Close - Open);

        public decimal UpperWick =>
            High - Math.Max(Open, Close);

        public decimal LowerWick =>
            Math.Min(Open, Close) - Low;

        public double BodyPct =>
            Range == 0
                ? 0
                : (double)(Body / Range);

        public double UpperWickPct =>
            Range == 0
                ? 0
                : (double)(UpperWick / Range);

        public double LowerWickPct =>
            Range == 0
                ? 0
                : (double)(LowerWick / Range);
    }
}
