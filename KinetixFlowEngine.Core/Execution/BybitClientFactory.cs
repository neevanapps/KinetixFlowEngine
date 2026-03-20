namespace KinetixFlowEngine.Core.Execution
{
    public class BybitClientFactory
    {
        private readonly Dictionary<string, BybitClientWrapper> _clients = new();

        public BybitClientWrapper GetClient(string accountId, string apiKey, string secret)
        {
            if (_clients.TryGetValue(accountId, out var client))
                return client;

            client = new BybitClientWrapper(apiKey, secret);
            _clients[accountId] = client;

            return client;
        }
    }
}