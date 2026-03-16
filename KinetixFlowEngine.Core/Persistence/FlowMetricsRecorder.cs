using KinetixFlowEngine.Core.Engine;
using System.Globalization;
using System.Text;

namespace KinetixFlowEngine.Core.Persistence
{
    public class FlowMetricsRecorder
    {
        private readonly string _filePath;

        public FlowMetricsRecorder()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "metrics");
            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "flow_metrics.csv");
            if (!File.Exists(_filePath))
            {
                WriteHeader();
            }
        }

        private void WriteHeader()
        {
            var header = "timestamp,price,score,score_z,velocity_z,imbalance_z,compression_z,exhaustion_z," +
                    "score_fast,score_medium,score_slow,price_trend,score_trend,state," +
                    "long_prob,short_prob,vwap,er5,er30,atr,oi," +
                    "delta_velocity,momentum,acceleration,persistence,size_bias,absorption," +
                    "buy_pressure,sell_pressure,net_pressure,bullish_absorption,bearish_distribution," +
                    "vwap_bullish_absorption,vwap_bearish_absorption,whale_buy_trades,whale_sell_trades," +
                    "impact_efficiency,bullish_control,bearish_control,prob_fast,prob_Medium,prob_slow,long_persist,short_persist,v15,v1";

            File.WriteAllText(_filePath, header + Environment.NewLine);
        }

        public void Record(KinetixEngineResult r)
        {
            var line = string.Join(",",
                DateTime.UtcNow.ToString("O"),
                r.Price.ToString(CultureInfo.InvariantCulture),
                r.AdjustedScore.ToString(CultureInfo.InvariantCulture),
                r.ScoreZ.ToString(CultureInfo.InvariantCulture),
                r.VelocityZ.ToString(CultureInfo.InvariantCulture),
                r.ImbalanceZ.ToString(CultureInfo.InvariantCulture),
                r.CompressionZ.ToString(CultureInfo.InvariantCulture),
                r.ExhaustionZ.ToString(CultureInfo.InvariantCulture),
                r.ScoreFastEma.ToString(CultureInfo.InvariantCulture),
                r.ScoreMediumEma.ToString(CultureInfo.InvariantCulture),
                r.ScoreSlowEma.ToString(CultureInfo.InvariantCulture),
                r.PriceTrend,
                r.ScoreTrend,
                r.FlowState.State,
                r.LongProbability.ToString(CultureInfo.InvariantCulture),
                r.ShortProbability.ToString(CultureInfo.InvariantCulture),
                r.VWAP.ToString(CultureInfo.InvariantCulture),
                r.ER.ToString(CultureInfo.InvariantCulture),
                r.ER30.ToString(CultureInfo.InvariantCulture),
                r.ATR.ToString(CultureInfo.InvariantCulture),
                r.OIChange.ToString(CultureInfo.InvariantCulture),
                r.DeltaVelocity.ToString(CultureInfo.InvariantCulture),
                r.Momentum.ToString(CultureInfo.InvariantCulture),
                r.Acceleration.ToString(CultureInfo.InvariantCulture),
                r.Persistence.ToString(CultureInfo.InvariantCulture),
                r.SizeBias.ToString(CultureInfo.InvariantCulture),
                r.Absorption.ToString(CultureInfo.InvariantCulture),
                r.BuyPressure.ToString(CultureInfo.InvariantCulture),
                r.SellPressure.ToString(CultureInfo.InvariantCulture),
                r.NetPressure.ToString(CultureInfo.InvariantCulture),
                r.BullishAbsorption,
                r.BearishDistribution,
                r.VwapBullishAbsorption,
                r.VwapBearishAbsorption,
                r.LargeBuyTrades,
                r.LargeSellTrades,
                r.FlowImpactEfficiency.ToString(CultureInfo.InvariantCulture),
                r.BullishPriceControl,
                r.BearishPriceControl,
                r.ProbFastEma.ToString(CultureInfo.InvariantCulture),
                r.ProbMediumEma.ToString(CultureInfo.InvariantCulture),
                r.ProbSlowEma.ToString(CultureInfo.InvariantCulture),
                r.LongPersistence,
                r.ShortPersistence,
                r.Volume15.ToString(CultureInfo.InvariantCulture),
                 r.Volume1.ToString(CultureInfo.InvariantCulture)
            );

            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}