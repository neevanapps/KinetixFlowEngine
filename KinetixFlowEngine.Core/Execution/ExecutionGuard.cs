using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Execution
{
    public class ExecutionGuard
    {
        private readonly ConcurrentDictionary<string, bool> _inFlight = new();

        public bool TryEnter(string accountId)
        {
            return _inFlight.TryAdd(accountId, true);
        }

        public void Exit(string accountId)
        {
            _inFlight.TryRemove(accountId, out _);
        }

        public bool IsBusy(string accountId)
        {
            return _inFlight.ContainsKey(accountId);
        }
    }
}
