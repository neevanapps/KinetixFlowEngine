using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Prop
{
    public class AccountStateEngine
    {
        
        public void UpdateState(
        PropAccountConfig config,
        PropAccountState state,
        decimal equity,
        DateTime nowUtc)
        {
            // -----------------------
            // DAILY RESET
            // -----------------------
            if (state.LastDailyResetUtc.Date != nowUtc.Date)
            {
                state.LastDailyResetUtc = nowUtc;

                state.DayStartEquity = equity;      // ✅ CRITICAL
                state.HighWaterMarkDaily = equity;

                state.IsPaused = false;
            }

            // -----------------------
            // INIT (first run)
            // -----------------------
            if (state.HighWaterMarkOverall == 0)
                state.HighWaterMarkOverall = equity;

            if (state.DayStartEquity == 0)
                state.DayStartEquity = equity;

            if (state.HighWaterMarkDaily == 0)
                state.HighWaterMarkDaily = equity;

            // -----------------------
            // UPDATE HIGH WATERMARKS
            // -----------------------
            state.HighWaterMarkOverall = Math.Max(state.HighWaterMarkOverall, equity);
            state.HighWaterMarkDaily = Math.Max(state.HighWaterMarkDaily, equity);

            // -----------------------
            // SELECT DAILY BASE
            // -----------------------
            decimal dailyBase =
                config.DailyDdMode == DailyDdMode.FromStartOfDay
                    ? state.DayStartEquity
                    : state.HighWaterMarkDaily;

            // SAFETY
            if (dailyBase == 0)
                dailyBase = equity;

            // -----------------------
            // DRAW DOWN CALCULATION
            // -----------------------
            state.DailyDrawdownPct =
                (dailyBase - equity) / dailyBase * 100;

            state.OverallDrawdownPct =
                (state.HighWaterMarkOverall - equity) / state.HighWaterMarkOverall * 100;

            // -----------------------
            // LIMIT FLAGS
            // -----------------------
            if (state.DailyDrawdownPct >= 3.5m)
                state.IsPaused = true;

            if (state.OverallDrawdownPct >= 10m)
                state.IsStopped = true;
        }
    }
}
