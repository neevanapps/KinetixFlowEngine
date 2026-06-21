using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KinetixFlowEngine.Core.Database
{
    public class ModelReviewEntity
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey(nameof(Snapshot))]
        public long SnapshotId { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }

        public string DirectionalBias { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        
        public int LongConfidence { get; set; }

        public int ShortConfidence { get; set; }

        public int Score { get; set; }

        public int TrendQuality { get; set; }

        public int FlowQuality { get; set; }

        public int RegimeQuality { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public string DominantIntent { get; set; } = string.Empty;

        public string MarketStructure { get; set; } = string.Empty;

        public string StateAssessment { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        // Store arrays as json
        public string BehaviorEvidenceJson { get; set; } = string.Empty;

        public string KeyDriversJson { get; set; } = string.Empty;

        public string ContradictionsJson { get; set; } = string.Empty;

        public string RawResponseJson { get; set; } = string.Empty;

        public decimal? FutureMove15m { get; set; }
        public decimal? FutureMove30m { get; set; }
        public decimal? FutureMove60m { get; set; }

        public bool? Correct15m { get; set; }
        public bool? Correct30m { get; set; }
        public bool? Correct60m { get; set; }
        public string Tradeability { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string RawResponse { get; set; } = string.Empty;
        public MarketSnapshotEntity Snapshot { get; set; } = null!;
    }
}
