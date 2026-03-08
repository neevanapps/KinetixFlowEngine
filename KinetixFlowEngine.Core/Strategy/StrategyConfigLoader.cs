using System.Text.Json;

namespace KinetixFlowEngine.Core.Strategy
{
    public class StrategyConfigLoader
    {
        private readonly Dictionary<string, StrategyConfig> _configs;

        public StrategyConfigLoader()
        {
            _configs = LoadConfigs();
        }

        public StrategyConfig Get(string strategyName)
        {
            if (_configs.TryGetValue(strategyName, out var config))
                return config;

            throw new Exception($"Strategy config not found: {strategyName}");
        }

        private Dictionary<string, StrategyConfig> LoadConfigs()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "strategy_config.json");

            if (!File.Exists(path))
                throw new Exception($"strategy_config.json not found at {path}");

            var json = File.ReadAllText(path);

            var configs = JsonSerializer.Deserialize<List<StrategyConfig>>(json)!;

            return configs.ToDictionary(x => x.StrategyName);
        }
    }
}