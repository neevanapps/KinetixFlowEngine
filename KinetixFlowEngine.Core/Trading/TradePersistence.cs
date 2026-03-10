using System.Text.Json;

namespace KinetixFlowEngine.Core.Trading
{
    public class TradePersistence
    {
        private readonly string _filePath =
            Path.Combine(AppContext.BaseDirectory, "active_trades.json");

        public Dictionary<string, ActiveTrade> Load()
        {
            if (!File.Exists(_filePath))
                return new Dictionary<string, ActiveTrade>();

            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<Dictionary<string, ActiveTrade>>(json)
                   ?? new Dictionary<string, ActiveTrade>();
        }

        public void Save(Dictionary<string, ActiveTrade> trades)
        {
            var json = JsonSerializer.Serialize(
                trades,
                new JsonSerializerOptions { WriteIndented = true });

            var temp = _filePath + ".tmp";

            File.WriteAllText(temp, json);
            File.Move(temp, _filePath, true);
        }

        public void Clear()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
    }
}