using KinetixFlowEngine.Core.Gpt.Configuration;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class DeepSeekReviewService
    : CloudReviewServiceBase,
      ICloudModelReviewer
{
    protected override string ModelName =>
        "deepseek-ai/deepseek-v4-flash";

    public DeepSeekReviewService(
        IGptReviewStore reviewStore,
        IGptPromptBuilder promptBuilder,
        IOptions<CloudAiSettings> settings,
        ILogger<DeepSeekReviewService> logger)
        : base(
            reviewStore,
            promptBuilder,
            settings,
            logger)
    {
    }
}