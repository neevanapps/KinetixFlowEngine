using KinetixFlowEngine.Core.Engine;
using System.Globalization;

namespace KinetixFlowEngine.Core.Persistence
{
    public class FlowMetricsRecorder
    {
        private readonly string _folder;
        private string _currentFilePath;
        private DateTime _currentDate;

        public FlowMetricsRecorder()
        {
            _folder = Path.Combine(AppContext.BaseDirectory, "metrics");
            Directory.CreateDirectory(_folder);

            _currentDate = DateTime.UtcNow.Date;
            _currentFilePath = GetFilePath(_currentDate);

            if (!File.Exists(_currentFilePath))
            {
                WriteHeader(_currentFilePath);
            }
        }

        private string GetFilePath(DateTime date)
        {
            return Path.Combine(_folder, $"flow_metrics_{date:yyyy-MM-dd}.csv");
        }

        private void EnsureFile()
        {
            var today = DateTime.UtcNow.Date;

            if (today != _currentDate)
            {
                _currentDate = today;
                _currentFilePath = GetFilePath(today);

                if (!File.Exists(_currentFilePath))
                {
                    WriteHeader(_currentFilePath);
                }
            }
        }

        private void WriteHeader(string path)
        {
            var header = "timestamp,price,score,score_z,velocity_z,imbalance_z,compression_z,exhaustion_z," +
                    "score_fast,score_medium,score_slow,price_trend,score_trend,state," +
                    "long_prob,short_prob,vwap,er5,er30,atr,oi," +
                    "delta_velocity,momentum,acceleration,persistence,size_bias,absorption," +
                    "buy_pressure,sell_pressure,net_pressure,bullish_absorption,bearish_distribution," +
                    "vwap_bullish_absorption,vwap_bearish_absorption,whale_buy_trades,whale_sell_trades," +
                    "impact_efficiency,bullish_control,bearish_control,prob_fast,prob_medium,prob_slow," +
                    "long_persist,short_persist,v15,v1,factor,long_stable,short_stable,vema," +

                    "sf_l1,sf_l2,sf_l3,sf_trend," +
                    "sm_l1,sm_l2,sm_l3,sm_trend," +
                    "ss_l1,ss_l2,ss_l3,ss_trend," +

                    "pf_l1,pf_l2,pf_l3,pf_trend," +
                    "pm_l1,pm_l2,pm_l3,pm_trend," +
                    "ps_l1,ps_l2,ps_l3,ps_trend";

            SafeWrite(path, header + Environment.NewLine);
        }

        private void SafeWrite(string path, string content)
        {
            var temp = path + ".tmp";
            File.WriteAllText(temp, content);
            File.Copy(temp, path, true);
            File.Delete(temp);
        }

        public void Record(KinetixEngineResult r)
        {
            EnsureFile();

            var s = r.EmaStability;

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
                r.Volume1.ToString(CultureInfo.InvariantCulture),

                r.TrendFactor.ToString(CultureInfo.InvariantCulture),
                r.LongStable,
                r.ShortStable,
                r.VelocityEma.ToString(CultureInfo.InvariantCulture),

                (s?.ScoreFastEmaLevel1 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ScoreFastEmaLevel2 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ScoreFastEmaLevel3 ?? 0).ToString(CultureInfo.InvariantCulture),
                s?.FastScoreTrend.ToString() ?? "Neutral",

                (s?.ScoreMediumEmaLevel1 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ScoreMediumEmaLevel2 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ScoreMediumEmaLevel3 ?? 0).ToString(CultureInfo.InvariantCulture),
                s?.MediumScoreTrend.ToString() ?? "Neutral",

                (s?.ScoreSlowEmaLevel1 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ScoreSlowEmaLevel2 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ScoreSlowEmaLevel3 ?? 0).ToString(CultureInfo.InvariantCulture),
                s?.SlowScoreTrend.ToString() ?? "Neutral",

                (s?.ProbFastEmaLevel1 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ProbFastEmaLevel2 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ProbFastEmaLevel3 ?? 0).ToString(CultureInfo.InvariantCulture),
                s?.FastProbTrend.ToString() ?? "Neutral",

                (s?.ProbMediumEmaLevel1 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ProbMediumEmaLevel2 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ProbMediumEmaLevel3 ?? 0).ToString(CultureInfo.InvariantCulture),
                s?.MediumProbTrend.ToString() ?? "Neutral",

                (s?.ProbSlowEmaLevel1 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ProbSlowEmaLevel2 ?? 0).ToString(CultureInfo.InvariantCulture),
                (s?.ProbSlowEmaLevel3 ?? 0).ToString(CultureInfo.InvariantCulture),
                s?.SlowProbTrend.ToString() ?? "Neutral"
            );

            File.AppendAllText(_currentFilePath, line + Environment.NewLine);
        }
    }
}