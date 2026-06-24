using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Configuration
{
    public sealed class CloudAiSettings
    {
        public string ApiKey { get; set; } = "csk-dtph56jv42265t5pthrt5d84pvy4exdh8xdt3r8fnjj4f4ce";

        public string BaseUrl { get; set; } =
            "https://api.cerebras.ai/v1/chat/completions";
    }
}
