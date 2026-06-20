using KinetixFlowEngine.Core.Gpt.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public interface IModelReviewer
    {
        string Name { get; }

        Task<GptReviewRecord> ReviewAsync(
            GptMarketSnapshotV2 snapshot,
            CancellationToken ct = default);
    }
}
