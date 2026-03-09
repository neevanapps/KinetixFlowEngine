using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Flow.Probability;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Engine
{
    public class KinetixEngineProcessor
    {
        private readonly FlowAggregationWindow _flowAggregationWindow;
        private readonly FlowFeatureEngine _flowFeatureEngine;
        private readonly FlowCompositeEngine _flowCompositeEngine;
        private readonly FlowScoreEngine _flowScoreEngine;
        private readonly FlowRegimeEngine _flowRegimeEngine;

        private readonly VwapEngine _vwapEngine;
        private readonly EfficiencyRatioEngine _erEngine;
        private readonly EfficiencyRatio30mEngine _er30m;
        private readonly AtrEngine _atrEngine;
        private readonly OpenInterestEngine _oiEngine;
        private readonly ContextScoreEngine _contextScoreEngine;
        private readonly Atr15mEngine _atr15m;

        private readonly PriceTrendEngine _priceEngine;
        private readonly ScoreTrendEngine _scoreEngine;

        private readonly FlowStateEngine _flowStateEngine;
        private readonly FlowProbabilityEngine _flowProbabilityEngine;
        private readonly SignalStabilityEngine _signalStabilityEngine;

        private readonly ScoreNormalizer _scoreNorm;
        private readonly VelocityNormalizer _velNorm;
        private readonly ImbalanceNormalizer _imbNorm;
        private readonly ExhaustionNormalizer _exhNorm;
        private readonly CompressionNormalizer _cmpNorm;

        private readonly OneMinuteCandleBuilder _candleBuilder = new();

        public KinetixEngineProcessor(
            FlowAggregationWindow flowAggregationWindow,
            FlowFeatureEngine flowFeatureEngine,
            FlowCompositeEngine flowCompositeEngine,
            FlowScoreEngine flowScoreEngine,
            FlowRegimeEngine flowRegimeEngine,
            VwapEngine vwapEngine,
            EfficiencyRatioEngine erEngine,
            AtrEngine atrEngine,
            OpenInterestEngine oiEngine,
            ContextScoreEngine contextScoreEngine,
            PriceTrendEngine priceEngine,
            ScoreTrendEngine scoreEngine,
            FlowStateEngine flowStateEngine,
            FlowProbabilityEngine flowProbabilityEngine,
            SignalStabilityEngine signalStabilityEngine,
            ScoreNormalizer scoreNorm,
            VelocityNormalizer velNorm,
            ImbalanceNormalizer imbNorm,
            ExhaustionNormalizer exhNorm,
            CompressionNormalizer cmpNorm,
            Atr15mEngine atr15m,
            EfficiencyRatio30mEngine er30m)
        {
            _flowAggregationWindow = flowAggregationWindow;
            _flowFeatureEngine = flowFeatureEngine;
            _flowCompositeEngine = flowCompositeEngine;
            _flowScoreEngine = flowScoreEngine;
            _flowRegimeEngine = flowRegimeEngine;

            _vwapEngine = vwapEngine;
            _erEngine = erEngine;
            _atrEngine = atrEngine;
            _oiEngine = oiEngine;
            _contextScoreEngine = contextScoreEngine;

            _priceEngine = priceEngine;
            _scoreEngine = scoreEngine;

            _flowStateEngine = flowStateEngine;
            _flowProbabilityEngine = flowProbabilityEngine;
            _signalStabilityEngine = signalStabilityEngine;

            _scoreNorm = scoreNorm;
            _velNorm = velNorm;
            _imbNorm = imbNorm;
            _exhNorm = exhNorm;
            _cmpNorm = cmpNorm;
            _atr15m = atr15m;
            _er30m = er30m;
        }

        public KinetixEngineResult Process(double price, decimal quantity, double openInterest)
        {
            var window = _flowAggregationWindow.GetSnapshot();

            var features = _flowFeatureEngine.Calculate(window, price);

            var composite = _flowCompositeEngine.Calculate(features);

            var score = _flowScoreEngine.CalculateScore(composite.CompositeSmoothed);

            var vwap = _vwapEngine.Update((decimal)price, quantity);

            var vwapDev = _vwapEngine.Deviation((decimal)price, vwap);

            var er5 = _erEngine.Update(price);
            var er30 = _er30m.Update(price);

            double atr = _atrEngine.Value;

            if (_candleBuilder.Update(price, out var candle))
            {
                atr = _atrEngine.Update(candle.High, candle.Low, candle.Close);
            }

            var oiChange = _oiEngine.Update(openInterest);

            var adjustedScore = _contextScoreEngine.AdjustScore(score, vwapDev, er30, oiChange);

            decimal priceDec = (decimal)price;
            decimal erDec = (decimal)er5;

            var priceTrend = _priceEngine.Update(priceDec, erDec);
            var scoreTrend = _scoreEngine.Update((decimal)adjustedScore, erDec);

            var scoreZ = _scoreNorm.Update(adjustedScore);
            var velZ = _velNorm.Update(features.DeltaVelocity);
            var imbZ = _imbNorm.Update(features.Imbalance);
            var exhZ = _exhNorm.Update(features.Exhaustion);
            var cmpZ = _cmpNorm.Update(features.Compression);

            var flowState = _flowStateEngine.Detect(
                scoreZ,
                velZ,
                imbZ,
                cmpZ,
                exhZ,
                features.Persistence,
                scoreTrend);

            var probability = _flowProbabilityEngine.Calculate(
                scoreZ,
                velZ,
                imbZ,
                cmpZ,
                exhZ,
                flowState,
                scoreTrend);

            bool longSignal = probability.LongProbability > 0.65 && scoreTrend == FlowTrend.Bullish;
            bool shortSignal = probability.ShortProbability > 0.65 && scoreTrend == FlowTrend.Bearish;

            var stability = _signalStabilityEngine.Update(longSignal, shortSignal);

            return new KinetixEngineResult
            {
                Price = price,
                RawScore = score,
                AdjustedScore = adjustedScore,
                ScoreZ = scoreZ,
                VelocityZ = velZ,
                ImbalanceZ = imbZ,
                ExhaustionZ = exhZ,
                CompressionZ = cmpZ,

                VWAP = (double)vwap,
                ER = er5,
                ER5 = er5,
                ER30 = er30,
                ATR = atr,
                OIChange = oiChange,
                ATR15m = _atr15m.Value,
                PriceTrend = priceTrend,
                ScoreTrend = scoreTrend,

                ScoreFastEma = (double)_scoreEngine.Fast,
                ScoreSlowEma = (double)_scoreEngine.Slow,
                ScoreMediumEma = (double)_scoreEngine.Medium,
                FlowState = flowState,

                LongProbability = probability.LongProbability,
                ShortProbability = probability.ShortProbability,

                LongStable = stability.LongStable,
                ShortStable = stability.ShortStable,
                LongPersistence = stability.LongPersistence,
                ShortPersistence = stability.ShortPersistence,

                // RAW FLOW FEATURES
                DeltaVelocity = features.DeltaVelocity,
                Momentum = features.Momentum,
                Acceleration = features.Acceleration,
                Persistence = features.Persistence,
                SizeBias = features.SizeBias,
                Absorption = features.Absorption
            };
        }
    }
}