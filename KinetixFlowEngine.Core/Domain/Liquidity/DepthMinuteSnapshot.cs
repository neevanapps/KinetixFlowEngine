using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.Liquidity
{
    public sealed class DepthMinuteSnapshot
    {
        /// <summary>
        /// UTC minute represented by this buffer.
        /// Example: 2026-06-25 10:42:00Z
        /// </summary>
        public DateTime MinuteUtc { get; init; }

        public DateTime StartUtc { get; init; }

        public DateTime EndUtc { get; init; }

        /// <summary>
        /// All depth snapshots received during this minute.
        /// </summary>
        public IReadOnlyList<DepthSnapshot> Snapshots { get; init; } = Array.Empty<DepthSnapshot>();

        /// <summary>
        /// Number of snapshots captured.
        /// </summary>
        public int Count => Snapshots.Count;
    }
}
