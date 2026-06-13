using KinetixFlowEngine.Core.Gpt.Configuration;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using KinetixFlowEngine.Core.Gpt.Validation;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public interface IGptReviewService
    {
        Task<GptReviewRecord> ReviewAsync(
            GptMarketSnapshotV2 snapshot,
            CancellationToken ct = default);
    }

    public sealed class GptReviewService : IGptReviewService
    {
        private readonly IGptReviewStore _reviewStore;
        private readonly IGptPromptBuilder _promptBuilder;
        private readonly ChatClient _chatClient;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                new JsonStringEnumConverter()
                }
            };

        public GptReviewService(
            IGptReviewStore reviewStore,
            IGptPromptBuilder promptBuilder,
            IOptions<GptSettings> settings)
        {
            _reviewStore = reviewStore;
            _promptBuilder = promptBuilder;

            var client =
                new OpenAIClient(settings.Value.ApiKey);

            _chatClient =
                client.GetChatClient(settings.Value.Model);
        }

        public async Task<GptReviewRecord> ReviewAsync(
            GptMarketSnapshotV2 snapshot,
            CancellationToken ct = default)
        {

            var prompt =
                _promptBuilder.BuildReviewPrompt(
                    snapshot);

            var messages = new List<ChatMessage>
                            {
                                new SystemChatMessage(        _promptBuilder.BuildSystemPrompt()),
                                new UserChatMessage(prompt)
                            };

            var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: ct);

            var rawResponse =
                completion.Value.Content[0].Text;
            GptAssessment? assessment = null;
            try
            {
                 assessment =
                    JsonSerializer.Deserialize<GptAssessment>(
                        rawResponse,
                        JsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to deserialize GPT assessment.",
                    ex);
            }

            if (assessment == null)
            {
                throw new InvalidOperationException(
                    "Failed to deserialize GPT assessment.");
            }
            GptAssessmentValidator.Validate(assessment);
            var review = new GptReviewRecord
            {
                CreatedUtc = DateTime.UtcNow,
                Sequence = snapshot.Sequence,
                Snapshot = snapshot,
                Assessment = assessment,
                RawResponse = rawResponse
            };

            await _reviewStore.AppendAsync(
                review,
                ct);

            return review;
        }
    }
}
