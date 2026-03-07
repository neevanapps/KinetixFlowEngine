using System.Text.Json;

namespace KinetixFlowEngine.Core.Persistence
{
    public class MarketStateManager
    {
        private const string FilePath = "market_state.json";

        public void Save(MarketStateSnapshot snapshot)
        {
            var json = JsonSerializer.Serialize(
                snapshot,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(FilePath, json);
        }

        public MarketStateSnapshot? Load()
        {
            if (!File.Exists(FilePath))
                return null;

            var json = File.ReadAllText(FilePath);

            return JsonSerializer.Deserialize<MarketStateSnapshot>(json);
        }
    }
}