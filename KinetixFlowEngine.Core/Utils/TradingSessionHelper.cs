
namespace KinetixFlowEngine.Core.Utils;

public static class TradingSessionHelper
{
    public static string GetSession(DateTime utc)
    {
        var hour = utc.Hour;

        if (hour >= 0 && hour < 7)
            return "Asia";

        if (hour >= 7 && hour < 12)
            return "Europe";

        if (hour >= 12 && hour < 16)
            return "Overlap";

        return "US";
    }
}
