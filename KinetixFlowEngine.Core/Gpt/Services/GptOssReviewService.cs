using KinetixFlowEngine.Core.Gpt.Configuration;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class GptOssReviewService
    : CloudReviewServiceBase,
      ICloudModelReviewer
    {
        protected override string ModelName =>
            "gpt-oss-120b";

        public GptOssReviewService(
            IGptReviewStore reviewStore,
            IGptPromptBuilder promptBuilder,
            IOptions<CloudAiSettings> settings,
            ILogger<GptOssReviewService> logger)
            : base(
                reviewStore,
                promptBuilder,
                settings,
                logger)
        {
        }
    }
}
