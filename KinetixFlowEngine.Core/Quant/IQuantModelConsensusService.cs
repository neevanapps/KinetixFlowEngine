namespace KinetixFlowEngine.Core.Quant;

public interface IQuantModelConsensusService
{
    Task<QuantModelConsensusDecision> GetLatestConsensusAsync(
        CancellationToken cancellationToken);
}