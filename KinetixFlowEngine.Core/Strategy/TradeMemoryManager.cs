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

            File.WriteAllText(_filePath, json);
        }

        // ✅ Called on trade close
        public void Record(TradeMemory trade)
        {
            _memory[trade.StrategyName] = trade;
            Save();
        }

        public TradeMemory? Get(string strategy)
        {
            _memory.TryGetValue(strategy, out var value);
            return value;
        }

        // ✅ Entry gate
        public bool IsBlocked(string strategy, SignalDirection direction)
        {
            if (!_memory.TryGetValue(strategy, out var last))
                return false;

            // Only block SAME direction
            if (last.Direction != direction)
                return false;

            // Only block SL / TSL
            if (last.ExitReason != "SL" && last.ExitReason != "TSL")
                return false;

            var elapsed = DateTime.UtcNow - last.ExitTime;

            return elapsed < Cooldown;
        }
    }
}
