using KinetixFlowEngine.Core.Gpt.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class LlmReviewMemory
    {
        private readonly object _lock = new();

        private readonly Dictionary<string, GptReviewRecord> _latest
            = new();

        public void Update(GptReviewRecord? review)
        {
            if (review == null)
                return;

            lock (_lock)
            {
                _latest[review.ModelName] = review;
            }
        }

        public IReadOnlyCollection<GptReviewRecord> GetLatest()
        {
            lock (_lock)
            {
                return _latest.Values.ToList();
            }
        }
    }
}
