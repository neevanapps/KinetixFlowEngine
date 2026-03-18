using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Prop
{
    public class PropAccountRuntimeFactory
    {
        private readonly List<AccountRuntime> _accounts;


        public PropAccountRuntimeFactory(IOptions<PropAccountsOptions> options, PropAccountStatePersistence persistence)
        {
            _accounts = new List<AccountRuntime>();

            foreach (var config in options.Value.PropAccounts)
            {
                // ---------- VALIDATION ----------
                if (string.IsNullOrWhiteSpace(config.AccountId))
                    throw new Exception("AccountId is required");

                if (config.StartingCapital <= 0)
                    throw new Exception($"Invalid capital for {config.AccountId}");

                if (config.LeverageCap <= 0)
                    throw new Exception($"Invalid leverage for {config.AccountId}");

                // Optional but recommended
                if (config.StrategyFilter == null)
                    config.StrategyFilter = Array.Empty<string>();

                var state = persistence.GetOrCreate(
                    config.AccountId,
                    config.StartingCapital);

                _accounts.Add(new AccountRuntime
                {
                    Config = config,
                    State = state,
                    Guard = new PropChallengeGuard()
                });
            }
        }

        public List<AccountRuntime> GetAccounts() => _accounts;
    }
}