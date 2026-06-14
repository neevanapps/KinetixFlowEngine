using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using KinetixFlowEngine.Core.Gpt.Validation;
using OllamaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class LocalLlmReviewService : IGptReviewService
    {
        private readonly IGptReviewStore _reviewStore;
        private readonly IGptPromptBuilder _promptBuilder;
        private readonly Chat _chat;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public LocalLlmReviewService(
            IGptReviewStore reviewStore,
            IGptPromptBuilder promptBuilder)
        {
            _reviewStore = reviewStore;
            _promptBuilder = promptBuilder;

            var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"))
            {
                SelectedModel = "qwen3:8b"
            };

            // Pass system prompt directly in Chat constructor
            var systemPrompt = _promptBuilder.BuildSystemPrompt();
            _chat = new Chat(ollamaClient, systemPrompt);
        }

        public async Task<GptReviewRecord> ReviewAsync(
            GptMarketSnapshotV2 snapshot,
            CancellationToken ct = default)
        {
            var userPrompt = _promptBuilder.BuildReviewPrompt(snapshot);

            string rawResponse = string.Empty;

            await foreach (var token in _chat.SendAsync(userPrompt, ct))
            {
                rawResponse += token;
            }

            if (string.IsNullOrWhiteSpace(rawResponse))
                throw new InvalidOperationException("Empty response from Local LLM.");

            GptAssessment? assessment = null;
            try
            {
                assessment = JsonSerializer.Deserialize<GptAssessment>(rawResponse, JsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize Local LLM assessment.\nRaw response: {rawResponse}", ex);
            }

            if (assessment == null)
                throw new InvalidOperationException("Failed to deserialize Local LLM assessment.");

            GptAssessmentValidator.Validate(assessment);

            var review = new GptReviewRecord
            {
                CreatedUtc = DateTime.UtcNow,
                Sequence = snapshot.Sequence,
                Snapshot = snapshot,
                Assessment = assessment,
                RawResponse = rawResponse
            };

            await _reviewStore.AppendAsync(review, ct);

            return review;
        }
    }
}