namespace KinetixFlowEngine.Core.Utils
{
    public class ScoreNormalizer : UniversalNormalizer
    {
        public ScoreNormalizer() : base(8640) // 12 hours @ 1 sec
        {
        }
    }

    public class VelocityNormalizer : UniversalNormalizer
    {
        public VelocityNormalizer() : base(8640)
        {
        }
    }

    public class ImbalanceNormalizer : UniversalNormalizer
    {
        public ImbalanceNormalizer() : base(8640)
        {
        }
    }

    public class ExhaustionNormalizer : UniversalNormalizer
    {
        public ExhaustionNormalizer() : base(8640)
        {
        }
    }

    public class CompressionNormalizer : UniversalNormalizer
    {
        public CompressionNormalizer() : base(8640) // 12 hours at 1s updates
        {
        }
    }
}