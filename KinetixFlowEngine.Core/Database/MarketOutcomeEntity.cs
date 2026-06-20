using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KinetixFlowEngine.Core.Database
{
    public class MarketOutcomeEntity
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey(nameof(Snapshot))]
        public long SnapshotId { get; set; }

        public DateTime CreatedUtc { get; set; }

        public decimal EntryPrice { get; set; }

        public decimal ATRAtReview { get; set; }

        // ---------------------------------
        // Future prices
        // ---------------------------------

        public decimal Price15m { get; set; }

        public decimal Price30m { get; set; }

        public decimal Price60m { get; set; }

        // ---------------------------------
        // Raw price movement
        // ---------------------------------

        public decimal Move15m { get; set; }

        public decimal Move30m { get; set; }

        public decimal Move60m { get; set; }

        // ---------------------------------
        // Percentage movement
        // ---------------------------------

        public double MovePct15m { get; set; }

        public double MovePct30m { get; set; }

        public double MovePct60m { get; set; }

        // ---------------------------------
        // ATR normalized movement
        // ---------------------------------

        public double MoveATR15m { get; set; }

        public double MoveATR30m { get; set; }

        public double MoveATR60m { get; set; }

        // ---------------------------------
        // Direction labels
        // ---------------------------------

        public string Direction15m { get; set; } = string.Empty;

        public string Direction30m { get; set; } = string.Empty;

        public string Direction60m { get; set; } = string.Empty;

        public MarketSnapshotEntity Snapshot { get; set; } = null!;
    }
}
