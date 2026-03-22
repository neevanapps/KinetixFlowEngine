using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Prop
{
    public class PropAccountRuntimeFactory
    {
        private readonly PropAccountStatePersistence _persistence;

        public PropAccountRuntimeFactory(PropAccountStatePersistence persistence)
        {
            _persistence = persistence;
        }

        public List<AccountRuntime> Create(PropAccountsOptions options)
        {
            var accounts = new List<AccountRuntime>();

            foreach (var config in options.PropAccounts)
            {
                if (!config.Enabled)
                    continue;

                // ---------- VALIDATION ----------
                if (string.IsNullOrWhiteSpace(config.AccountId))
                    throw new Exception("AccountId is required");

                if (config.StartingCapital <= 0)
                    throw new Exception($"Invalid capital for {config.AccountId}");

                if (config.LeverageCap <= 0)
                    throw new Exception($"Invalid leverage for {config.AccountId}");

                if (config.StrategyFilter == null)
                    config.StrategyFilter = Array.Empty<string>();

                var state = _persistence.GetOrCreate(
                    config.AccountId,
                    config.StartingCapital);

                accounts.Add(new AccountRuntime
                {
                    Config = config,
                    State = state,
                    Guard = new PropChallengeGuard()
                });
            }

            return accounts;
        }
    }
}