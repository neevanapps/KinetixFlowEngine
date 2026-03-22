using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Prop
{
    using Microsoft.Extensions.Options;


    public class PropAccountRuntimeManager
    {
        private readonly PropAccountRuntimeFactory _factory;
        private readonly object _lock = new();
        private readonly ILogger<PropAccountRuntimeManager> _logger;
        private List<AccountRuntime> _accounts = new();

        public IReadOnlyList<AccountRuntime> Accounts => _accounts;

        public PropAccountRuntimeManager(
            IOptionsMonitor<PropAccountsOptions> monitor,
            PropAccountRuntimeFactory factory,
            ILogger<PropAccountRuntimeManager> logger)
        {
            _factory = factory;
            _logger = logger;
            // Initial load
            _accounts = _factory.Create(monitor.CurrentValue);

            monitor.OnChange(OnConfigChanged);
        }

        private void OnConfigChanged(PropAccountsOptions options)
        {
            try
            {
                var newAccounts = _factory.Create(options);

                lock (_lock)
                {
                    MergeState(_accounts, newAccounts);
                    _accounts = newAccounts;
                    _logger.LogInformation("Config reload successfully.");
                }
            }
            catch (Exception ex)
            {
                // VERY IMPORTANT: do not break engine on bad config
                Console.WriteLine($"Config reload failed: {ex.Message}");
            }
        }

        private void MergeState(
            List<AccountRuntime> oldList,
            List<AccountRuntime> newList)
        {
            foreach (var newAcc in newList)
            {
                var old = oldList.FirstOrDefault(x => x.Config.AccountId == newAcc.Config.AccountId);
                if (old == null)
                    continue;

                // Preserve runtime state
                newAcc.State = old.State;

                // Preserve guard (important for prop rules)
                newAcc.Guard = old.Guard;
            }
        }
    }

}
