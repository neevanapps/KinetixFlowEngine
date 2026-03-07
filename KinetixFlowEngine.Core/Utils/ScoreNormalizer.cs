namespace KinetixFlowEngine.Core.Utils
{
    public class ScoreNormalizer : UniversalNormalizer
    {
        public ScoreNormalizer() : base(43200) // 12 hours @ 1 sec
        {
        }
    }

    public class VelocityNormalizer : UniversalNormalizer
    {
        public VelocityNormalizer() : base(43200)
        {
        }
    }

    public class ImbalanceNormalizer : UniversalNormalizer
    {
        public ImbalanceNormalizer() : base(43200)
        {
        }
    }

    public class ExhaustionNormalizer : UniversalNormalizer
    {
        public ExhaustionNormalizer() : base(43200)
        {
        }
    }

    public class CompressionNormalizer : UniversalNormalizer
    {
        public CompressionNormalizer() : base(43200) // 12 hours at 1s updates
        {
        }
    }
}