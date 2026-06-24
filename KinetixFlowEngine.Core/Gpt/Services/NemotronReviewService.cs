using KinetixFlowEngine.Core.Gpt.Configuration;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class NemotronReviewService
    : CloudReviewServiceBase,
      ICloudModelReviewer
{
    protected override string ModelName =>
        "nvidia/llama-nemotron-super-v1.5";

    public NemotronReviewService(
        IGptReviewStore reviewStore,
        IGptPromptBuilder promptBuilder,
        IOptions<CloudAiSettings> settings,
        ILogger<NemotronReviewService> logger)
        : base(
            reviewStore,
            promptBuilder,
            settings,
            logger)
    {
    }
}