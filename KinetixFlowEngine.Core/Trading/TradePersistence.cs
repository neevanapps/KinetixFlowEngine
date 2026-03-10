using System.Text.Json;

namespace KinetixFlowEngine.Core.Trading
{
    public class TradePersistence
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "active_trade.json");

        public void Save(ActiveTrade trade)
        {
            var json = JsonSerializer.Serialize(trade, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }

        public ActiveTrade? Load()
        {
            if (!File.Exists(_filePath))
                return null;

            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<ActiveTrade>(json);
        }

        public void Clear()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
    }
}