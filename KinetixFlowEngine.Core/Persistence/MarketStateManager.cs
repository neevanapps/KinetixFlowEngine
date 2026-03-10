using System.Text.Json;

namespace KinetixFlowEngine.Core.Persistence
{
    public class MarketStateManager
    {
        private readonly string _filePath =
    Path.Combine(AppContext.BaseDirectory, "market_state.json");

        public void Save(MarketStateSnapshot snapshot)
        {
            var json = JsonSerializer.Serialize(
                snapshot,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            var temp = _filePath + ".tmp";

            File.WriteAllText(temp, json);
            File.Move(temp, _filePath, true);
        }

        public MarketStateSnapshot? Load()
        {
            if (!File.Exists(_filePath))
                return null;

            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<MarketStateSnapshot>(json);
        }
    }
}