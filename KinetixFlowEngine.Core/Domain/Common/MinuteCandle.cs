using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.Common
{
    public sealed class MinuteCandle
    {
        public DateTime MinuteUtc { get; init; }

        public decimal Open { get; init; }

        public decimal High { get; init; }

        public decimal Low { get; init; }

        public decimal Close { get; init; }

        public decimal Range => High - Low;

        public decimal Body => Math.Abs(Close - Open);

        public decimal UpperWick =>
            High - Math.Max(Open, Close);

        public decimal LowerWick =>
            Math.Min(Open, Close) - Low;

        public decimal BodyPct =>
            Range == 0
                ? 0
                : Body / Range * 100m;

        public decimal UpperWickPct =>
            Range == 0
                ? 0
                : UpperWick / Range * 100m;

        public decimal LowerWickPct =>
            Range == 0
                ? 0
                : LowerWick / Range * 100m;
    }
}
