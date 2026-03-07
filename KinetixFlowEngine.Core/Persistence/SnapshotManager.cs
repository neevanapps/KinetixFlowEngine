using System.Text.Json;

namespace KinetixFlowEngine.Core.Persistence
{
    public class SnapshotManager
    {
        private const string FilePath = "engine_snapshot.json";

        public void Save(EngineSnapshot snapshot)
        {
            var json = JsonSerializer.Serialize(snapshot,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(FilePath, json);
        }

        public EngineSnapshot? Load()
        {
            if (!File.Exists(FilePath))
                return null;

            var json = File.ReadAllText(FilePath);

            return JsonSerializer.Deserialize<EngineSnapshot>(json);
        }
    }
}