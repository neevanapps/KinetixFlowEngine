using KinetixFlowEngine.Core.Gpt.Configuration;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class GlmResvireService
    : CloudReviewServiceBase,
      ICloudModelReviewer
    {
        protected override string ModelName =>
            "zai-glm-4.7";

        public GlmResvireService(
            IGptReviewStore reviewStore,
            IGptPromptBuilder promptBuilder,
            IOptions<CloudAiSettings> settings,
            ILogger<GlmResvireService> logger)
            : base(
                reviewStore,
                promptBuilder,
                settings,
                logger)
        {
        }
    }
}
//