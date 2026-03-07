using KinetixFlowEngine.Core.Utils;
using KinetixFlowEngine.Core.Context;

namespace KinetixFlowEngine.Core.Engine
{
    public class EngineWarmupManager
    {
        private readonly ScoreNormalizer _scoreNorm;
        private readonly VelocityNormalizer _velNorm;
        private readonly ImbalanceNormalizer _imbNorm;
        private readonly ExhaustionNormalizer _exhNorm;
        private readonly CompressionNormalizer _cmpNorm;
        private readonly AtrEngine _atr;

        private readonly DateTime _engineStart;

        private const int MinimumRuntimeSeconds = 120;

        public bool IsReady { get; private set; }

        public EngineWarmupManager(
            ScoreNormalizer scoreNorm,
            VelocityNormalizer velNorm,
            ImbalanceNormalizer imbNorm,
            ExhaustionNormalizer exhNorm,
            CompressionNormalizer cmpNorm,
            AtrEngine atr)
        {
            _scoreNorm = scoreNorm;
            _velNorm = velNorm;
            _imbNorm = imbNorm;
            _exhNorm = exhNorm;
            _cmpNorm = cmpNorm;
            _atr = atr;

            _engineStart = DateTime.UtcNow;
        }

        public bool Update()
        {
            if (IsReady)
                return true;

            bool normalizersReady =
                _scoreNorm.IsReady &&
                _velNorm.IsReady &&
                _imbNorm.IsReady &&
                _exhNorm.IsReady &&
                _cmpNorm.IsReady;

            bool atrReady = _atr.IsReady;

            IsReady = normalizersReady && atrReady;

            return IsReady;
        }
    }
}