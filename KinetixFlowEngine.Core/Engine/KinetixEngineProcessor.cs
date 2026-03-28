using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Flow.Probability;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Models;
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
        private readonly FlowTradeBuffer _tradeBuffer;

        private readonly VwapEngine _vwapEngine;
        private readonly EfficiencyRatioEngine _erEngine;
        private readonly EfficiencyRatio30mEngine _er30m;
        private readonly AtrEngine _atrEngine;
        private readonly OpenInterestEngine _oiEngine;
        private readonly ContextScoreEngine _contextScoreEngine;
        private readonly Atr15mEngine _atr15m;
        private readonly FifteenMinuteCandleBuilder _candle15m = new();
        private readonly PriceTrendEngine _priceEngine;
        private readonly ScoreTrendEngine _scoreEngine;
        private readonly ProbabilityTrendEngine _probEngine;
        private readonly FlowDivergenceEngine _divergenceEngine;
        private readonly FlowStateEngine _flowStateEngine;
        private readonly FlowProbabilityEngine _flowProbabilityEngine;
        private readonly SignalStabilityEngine _signalStabilityEngine;
        private readonly LiquidityPressureEngine _pressureEngine;
        private readonly VwapAbsorptionEngine _vwapAbsorptionEngine;
        private readonly WhaleClusterEngine _whaleClusterEngine;
        private readonly FlowPersistenceEngine _flowPersistenceEngine;
        private readonly FlowImpactEngine _flowImpactEngine;
        private double _previousPrice;
        private readonly VolumeEngine _volumeEngine;
        private readonly ScoreNormalizer _scoreNorm;
        private readonly VelocityNormalizer _velNorm;
        private readonly ImbalanceNormalizer _imbNorm;
        private readonly ExhaustionNormalizer _exhNorm;
        private readonly CompressionNormalizer _cmpNorm;
        private readonly Ema _velocityEma = new(30);
        public double VelocityEma;
        private readonly FlowMomentumRun _momentumRun;
        private readonly EmaStability _emaStability;
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
            FlowDivergenceEngine divergenceEngine,
            EfficiencyRatio30mEngine er30m,
            LiquidityPressureEngine pressureEngine,
            VwapAbsorptionEngine vwapAbsorptionEngine,
            WhaleClusterEngine whaleClusterEngine, FlowImpactEngine flowImpactEngine,
            FlowPersistenceEngine flowPersistenceEngine, ProbabilityTrendEngine probEngine, FlowTradeBuffer tradeBuffer, FifteenMinuteCandleBuilder candle15m, VolumeEngine volumeEngine, FlowMomentumRun momentumRun, EmaStability emaStability)
        {
            _flowAggregationWindow = flowAggregationWindow;
            _flowFeatureEngine = flowFeatureEngine;
            _flowCompositeEngine = flowCompositeEngine;
            _flowScoreEngine = flowScoreEngine;
            _flowRegimeEngine = flowRegimeEngine;
            _flowImpactEngine = flowImpactEngine;
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
            _divergenceEngine = divergenceEngine;

            _scoreNorm = scoreNorm;
            _velNorm = velNorm;
            _imbNorm = imbNorm;
            _exhNorm = exhNorm;
            _cmpNorm = cmpNorm;
            _atr15m = atr15m;
            _er30m = er30m;
            _pressureEngine = pressureEngine;
            _vwapAbsorptionEngine = vwapAbsorptionEngine;
            _whaleClusterEngine = whaleClusterEngine;
            _flowPersistenceEngine = flowPersistenceEngine;
            _probEngine = probEngine;
            _tradeBuffer = tradeBuffer;
            _candle15m = candle15m;
            _volumeEngine = volumeEngine;
            _momentumRun = momentumRun;
            _emaStability = emaStability;
        }

        public KinetixEngineResult Process(double price, decimal quantity, long timeStamp, double openInterest, RollingWindowBuffer _scoreFast, RollingWindowBuffer _scoreMedium,
                            RollingWindowBuffer _scoreSlow, RollingWindowBuffer _probFast, RollingWindowBuffer _probMedium, RollingWindowBuffer _probSlow)
        {
            var allTrades = _tradeBuffer.GetSnapshot();
            long cutoff = DateTimeOffset.UtcNow.AddSeconds(-60).ToUnixTimeMilliseconds();
            var whaleClusters = _whaleClusterEngine.Detect(allTrades, cutoff);

            var window = _flowAggregationWindow.GetSnapshot();

            _volumeEngine.Update((double)quantity);

            var features = _flowFeatureEngine.Calculate(window, price, _previousPrice, _atrEngine.Value);

            var composite = _flowCompositeEngine.Calculate(features);

            var score = _flowScoreEngine.CalculateScore(composite.CompositeSmoothed);

            var vwap = _vwapEngine.Update((decimal)price, quantity);

            var vwapDev = _vwapEngine.Deviation((decimal)price, vwap);

            var er5 = _erEngine.Update(price);
            var er30 = _er30m.Update(price);

            double atr = _atrEngine.Value;

            if (_candleBuilder.Update(price, timeStamp, out var candle))
            {
                atr = _atrEngine.Update(candle.High, candle.Low, candle.Close);
            }
            if (_candle15m.Update(price, timeStamp, out var candle15))
            {
                _atr15m.Update(candle15.High, candle15.Low, candle15.Close);
            }
            var impact = _flowImpactEngine.Calculate(price, _previousPrice, window, atr);
            _previousPrice = price;

            var oiChange = _oiEngine.Update(openInterest);

            decimal priceDec = (decimal)price;
            decimal erDec = (decimal)er5;

            var priceTrend = _priceEngine.Update(priceDec, erDec);
            var pressure = _pressureEngine.Calculate(window, price, atr, (double)vwap);

            var contextScore = _contextScoreEngine.AdjustScore(score, vwapDev, er30, oiChange, priceTrend);

            var baseAlpha = AdaptiveAlpha.Compute(atr, er5);
            var factor = (double)_momentumRun.LastFactor;
            var alpha = baseAlpha * (0.9 + 0.2 * factor);
            alpha = Math.Clamp(alpha, 0.04, 0.35);

            var velZ = _velNorm.Update(features.DeltaVelocity, alpha);
            velZ = Math.Clamp(velZ, -2, 2);
            VelocityEma = _velocityEma.Update(velZ);

            // temporary trend proxy (no EMA update)
            var tempTrend = contextScore > 0 ? FlowTrend.Bullish : contextScore < 0 ? FlowTrend.Bearish : FlowTrend.Neutral;
            var divergence = _divergenceEngine.Detect(priceTrend, tempTrend, contextScore, vwapDev);
            var vwapAbsorption = _vwapAbsorptionEngine.Detect(price, (double)vwap, contextScore, priceTrend);

            bool bearishTrap = divergence.BearishDistribution || vwapAbsorption.BearishAbsorption;
            bool bullishTrap = divergence.BullishAbsorption || vwapAbsorption.BullishAbsorption;

            var imbZ = _imbNorm.Update(features.Imbalance, alpha);
            var exhZ = _exhNorm.Update(features.Exhaustion, alpha);
            var cmpZ = _cmpNorm.Update(features.Compression, alpha);

            bool highPersistence = features.Persistence > 4.0;
            bool volumeExpansion = _volumeEngine.IsVolumeExpansion();
            var adjustedScore = _contextScoreEngine.ApplyStructureFilter(contextScore, impact, bearishTrap, bullishTrap, highPersistence, volumeExpansion);

            double atrNorm = Math.Max(atr, 1); // avoid divide by zero
            double finalScore = adjustedScore / (atrNorm / 100.0);
            finalScore = Math.Clamp(finalScore, -20, 20);

            // OPTIONAL: keep scoreZ only for logging (DO NOT USE IN LOGIC)
            var scoreZ = _scoreNorm.Update(finalScore, alpha);

            var scoreTrend = _scoreEngine.Update((decimal)finalScore, velZ, highPersistence, volumeExpansion);
            var flowState = _flowStateEngine.Detect(finalScore, velZ, imbZ, cmpZ, exhZ, features.Persistence, scoreTrend);

            var probability = _flowProbabilityEngine.Calculate(finalScore, velZ, features.Persistence, _scoreEngine.Fast, _scoreEngine.Medium, _scoreEngine.Slow,
                                        volumeExpansion, exhZ, impact.BullishControl, impact.BearishControl);

            var probAlpha = Math.Clamp(alpha * 1.2, 0.05, 0.4);
            var ProbTrend = _probEngine.Update((decimal)probability.LongProbability, velZ, highPersistence, volumeExpansion);

            // ===== Momentum Gate (NEW) =====
            bool momentumExpansion = Math.Abs((decimal)_velocityEma.Value) > 0.8m;
            bool directionAligned = (_scoreEngine.Medium > 0 && _velocityEma.Value > 0) || (_scoreEngine.Medium < 0 && _velocityEma.Value < 0);
            bool tradeGate = momentumExpansion && directionAligned;

            // ===== Existing Trend Filter =====
            bool strongTrend = Math.Abs(_scoreEngine.Medium) > 1.0m && Math.Abs(_scoreEngine.Fast - _scoreEngine.Slow) > 1.2m;
            // ===== Final Signals =====
            bool longSignal = probability.LongProbability > 0.65 && scoreTrend == FlowTrend.Bullish && strongTrend && tradeGate;
            bool shortSignal = probability.ShortProbability > 0.65 && scoreTrend == FlowTrend.Bearish && strongTrend && tradeGate;
            var stability = _signalStabilityEngine.Update(longSignal, shortSignal);

            var emaStabilityState = _emaStability.Compute(_scoreFast, _scoreMedium, _scoreSlow, _probFast, _probMedium, _probSlow);
            // ===== Momentum Decay Detection (NEW) =====
            bool momentumDying = Math.Abs((decimal)_velocityEma.Value) < 0.3m;
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
                Volume15 = _volumeEngine.Sum,
                Volume1 = _volumeEngine.Average,
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
                ShortProbability = 1 - probability.LongProbability,

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
                Absorption = features.Absorption,

                BullishAbsorption = divergence.BullishAbsorption,
                BearishDistribution = divergence.BearishDistribution,
                DivergenceStrength = divergence.Strength,
                BuyPressure = pressure.BuyPressure,
                SellPressure = pressure.SellPressure,
                NetPressure = pressure.NetPressure,
                BullishBreakout = pressure.BullishBreakout,
                BearishBreakout = pressure.BearishBreakout,
                VwapBullishAbsorption = vwapAbsorption.BullishAbsorption,
                VwapBearishAbsorption = vwapAbsorption.BearishAbsorption,
                VwapAbsorptionStrength = vwapAbsorption.Strength,
                LargeBuyTrades = whaleClusters.LargeBuyTrades,
                LargeSellTrades = whaleClusters.LargeSellTrades,
                BuyClusterStrength = whaleClusters.BuyClusterStrength,
                SellClusterStrength = whaleClusters.SellClusterStrength,
                FlowImpactEfficiency = impact.Efficiency,
                BullishPriceControl = impact.BullishControl,
                BearishPriceControl = impact.BearishControl,
                ProbFastEma = (double)_probEngine.Fast,
                ProbSlowEma = (double)_probEngine.Slow,
                ProbMediumEma = (double)_probEngine.Medium,
                TrendFactor = factor,
                VelocityEma = _velocityEma.Value,
                MomentumDying = momentumDying,
                TradeGate = tradeGate,
                EmaStability = emaStabilityState
            };
        }
    }
}