using System.Diagnostics;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Trading
{
    public class PositionPersistence
    {
        private readonly string _filePath;

        public PositionPersistence()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "persist");
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, "positions.json");
        }

        public void Save(IEnumerable<ActiveTrade> trades)
        {
            var data = trades.Select(t => new PersistedPosition
            {
                AccountId = t.AccountId,
                StrategyName = t.StrategyName,
                Direction = t.Direction,
                EntryPrice = t.EntryPrice,
                StopLoss = t.StopLoss,
                Target1 = t.Target1,
                Size = t.InitialSize,
                EntryTimeMs = t.EntryTimeMs,
                Target1Hit = t.Target1Hit,
                MaxPrice = t.MaxPrice,
                MinPrice = t.MinPrice,
                EntryScoreZ = t.EntryScoreZ,
                EntryVelocityZ = t.EntryVelocityZ,
                EntryImbalanceZ = t.EntryImbalanceZ,
                EntryCompressionZ = t.EntryCompressionZ,
                EntryATR = t.EntryATR,
                EntryER = t.EntryER,
                EntryFlowState = t.EntryFlowState.ToString(),
                EntryPriceTrend = t.EntryPriceTrend,
                EntryScoreTrend = t.EntryScoreTrend,
                OrderId = t.OrderId,
                RemainingSize = t.RemainingSize,
                EntryAlertSent = t.EntryAlertSent
            }).ToList();

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var tempFile = _filePath + ".tmp";

            File.WriteAllText(tempFile, json);

            if (File.Exists(_filePath))
                File.Delete(_filePath);

            File.Move(tempFile, _filePath);
        }

        public List<PersistedPosition> Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<PersistedPosition>();

                var json = File.ReadAllText(_filePath);

                return JsonSerializer.Deserialize<List<PersistedPosition>>(json)
                       ?? new List<PersistedPosition>();
            }
            catch
            {
                File.Move(_filePath, _filePath + ".corrupt_" + DateTime.UtcNow.Ticks);
                return new List<PersistedPosition>();
            }
        }
    }
}