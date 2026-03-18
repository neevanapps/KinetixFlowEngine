using System.Text.Json;

namespace KinetixFlowEngine.Core.Strategy
{
    public class TradeMemoryManager
    {
        private readonly string _filePath;
        private readonly Dictionary<string, TradeMemory> _memory;

        private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(15);

        public TradeMemoryManager()
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, "trade_memory.json");
            _memory = Load();
        }

        private Dictionary<string, TradeMemory> Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new Dictionary<string, TradeMemory>();

                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<Dictionary<string, TradeMemory>>(json)
                       ?? new Dictionary<string, TradeMemory>();
            }
            catch
            {
                return new Dictionary<string, TradeMemory>();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_memory, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var tempFile = _filePath + ".tmp";

            File.WriteAllText(tempFile, json);

            if (File.Exists(_filePath))
                File.Delete(_filePath);

            File.Move(tempFile, _filePath);
        }

        // ✅ Called on trade close
        public void Record(string accountId, TradeMemory trade)
        {
            var key = GetKey(trade.StrategyName, accountId);
            _memory[key] = trade;
            Save();
        }

        public TradeMemory? Get(string strategy, string accountId)
        {
            _memory.TryGetValue(GetKey(strategy, accountId), out var value);
            return value;
        }

        private static string GetKey(string strategy, string accountId) => $"{accountId}::{strategy}";

        // ✅ Entry gate
        public bool IsBlocked(string strategy, string accountId, SignalDirection direction)
        {
            var key = GetKey(strategy, accountId);

            if (!_memory.TryGetValue(key, out var last))
                return false;

            if (last.Direction != direction)
                return false;

            if (last.ExitReason != "SL" && last.ExitReason != "TSL")
                return false;

            var elapsed = DateTime.UtcNow - last.ExitTime;

            return elapsed < Cooldown;
        }
    }
}
