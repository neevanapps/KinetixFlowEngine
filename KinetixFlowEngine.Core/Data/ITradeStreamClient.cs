using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Data
{
    public interface ITradeStreamClient
    {
        event Action<FlowTrade>? OnTrade;
        Task StartAsync(CancellationToken cancellationToken);
    }
}
