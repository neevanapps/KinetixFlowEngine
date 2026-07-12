namespace KinetixFlowEngine.Core.Quant;

public interface IQuantModelDecisionReader
{
    Task<IReadOnlyList<QuantModelDecisionBatch>> GetLatestCompleteBatchesAsync(
        CancellationToken cancellationToken);

    Task<QuantModelDecisionBatch?> GetLatestCompleteBatchAsync(
        CancellationToken cancellationToken);
}