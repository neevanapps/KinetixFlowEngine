using System.Text.Json;

namespace KinetixFlowEngine.Core.Prop
{
    public class PropAccountStatePersistence
    {
        private readonly string _filePath;
        private Dictionary<string, PropAccountState> _cache;

        public PropAccountStatePersistence()
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, "prop_account_state.json");
            _cache = LoadAll();
        }

        // -----------------------------
        // PUBLIC LOAD PER ACCOUNT
        // -----------------------------
        public PropAccountState? Load(string accountId)
        {
            _cache.TryGetValue(accountId, out var state);
            return state;
        }

        // -----------------------------
        // UPDATE (SAVE)
        // -----------------------------
        public void Update(string accountId, PropAccountState state)
        {
            _cache[accountId] = state;
            SaveAll();
        }

        public PropAccountState GetOrCreate(string accountId, decimal startingCapital)
        {
            if (_cache.TryGetValue(accountId, out var state))
                return state;

            var newState = new PropAccountState
            {
                CurrentEquity = startingCapital,
                HighWaterMarkDaily = startingCapital,
                HighWaterMarkOverall = startingCapital
            };

            _cache[accountId] = newState;
            SaveAll();

            return newState;
        }

        // -----------------------------
        // INTERNAL LOAD ALL
        // -----------------------------
        private Dictionary<string, PropAccountState> LoadAll()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new Dictionary<string, PropAccountState>();

                var json = File.ReadAllText(_filePath);

                return JsonSerializer.Deserialize<Dictionary<string, PropAccountState>>(json)
                       ?? new Dictionary<string, PropAccountState>();
            }
            catch
            {
                return new Dictionary<string, PropAccountState>();
            }
        }

        // -----------------------------
        // SAFE SAVE
        // -----------------------------
        private void SaveAll()
        {
            var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var temp = _filePath + ".tmp";

            File.WriteAllText(temp, json);

            if (File.Exists(_filePath))
                File.Delete(_filePath);

            File.Move(temp, _filePath);
        }
    }
}