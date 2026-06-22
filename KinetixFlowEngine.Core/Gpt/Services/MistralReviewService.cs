using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using KinetixFlowEngine.Core.Gpt.Validation;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class MistralReviewService : IModelReviewer
    {
        private readonly IGptReviewStore _reviewStore;
        private readonly IGptPromptBuilder _promptBuilder;
        private readonly OllamaApiClient _ollamaClient;
        private readonly ILogger<MistralReviewService> _logger;
        private const string ModelName = "mistral-small:latest";
        public string Name => ModelName;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public MistralReviewService(
            IGptReviewStore reviewStore,
            IGptPromptBuilder promptBuilder,
            ILogger<MistralReviewService> logger)
        {
            _reviewStore = reviewStore;
            _promptBuilder = promptBuilder;
            _logger = logger;

            _ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"))
            {
                SelectedModel = ModelName
            };
        }

        public async Task<GptReviewRecord> ReviewAsync(
    GptMarketSnapshotV2 snapshot,
    CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();

            var systemPrompt = _promptBuilder.BuildSystemPrompt();
            var userContent = _promptBuilder.BuildReviewPrompt(snapshot);

            _logger.LogInformation(
               "Reviewing system prompt: {Prompt}",
               systemPrompt);

            _logger.LogInformation(
                "Reviewing snapshot {Sequence} with prompt: {Prompt}",
                snapshot.Sequence,
                userContent);

            var chatRequest = new ChatRequest
            {
                Model = _ollamaClient.SelectedModel,
                Messages = new List<Message>
        {
            new Message { Role = ChatRole.System, Content = systemPrompt },
            new Message { Role = ChatRole.User, Content = userContent }
        },
                Stream = true,
                Format = "json",                    // ← Strongly recommended for Qwen3
                Think = false,
                Options = new RequestOptions
                {
                    Temperature = 0.0f,
                    TopP = 0.1f,
                    NumCtx = 4096,
                    NumPredict = 1200,
                    RepeatPenalty = 1.15f
                }
            };

            string rawResponse = string.Empty;

            try
            {
                await foreach (var chunk in _ollamaClient.ChatAsync(chatRequest, ct))
                {
                    if (chunk?.Message?.Content != null)
                    {
                        rawResponse += chunk.Message.Content;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ollama ChatAsync failed for Seq:{Seq}", snapshot.Sequence);
                throw;
            }

            stopwatch.Stop();

            _logger?.LogInformation("LLM Inference Time | Seq:{Seq} | {Time}ms",
                snapshot.Sequence, stopwatch.ElapsedMilliseconds);

            _logger?.LogInformation("Raw LLM Response | Seq:{Seq} | {Response}", snapshot.Sequence, rawResponse);

            if (string.IsNullOrWhiteSpace(rawResponse))
                throw new InvalidOperationException($"Empty response from Local LLM for sequence {snapshot.Sequence}. Check model name and prompt.");

            // === Your existing logic continues unchanged ===
            GptAssessment? assessment = null;
            try
            {
                var cleanJson = CleanMarkdownWrappers(rawResponse);
                assessment = JsonSerializer.Deserialize<GptAssessment>(cleanJson, JsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize Local LLM assessment.\nRaw response: {rawResponse}", ex);
            }

            if (assessment == null)
                throw new InvalidOperationException("Failed to deserialize Local LLM assessment.");

            assessment = NormalizeAssessment(assessment);
            GptAssessmentValidator.Validate(assessment);

            var review = new GptReviewRecord
            {
                CreatedUtc = DateTime.UtcNow,
                Sequence = snapshot.Sequence,
                ModelName = ModelName,
                Snapshot = snapshot,
                Assessment = assessment,
                RawResponse = rawResponse
            };

            await _reviewStore.AppendAsync(review, ct);
            return review;
        }

        private static string CleanMarkdownWrappers(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var trimmed = input.Trim();
            if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                trimmed = trimmed[7..];
            else if (trimmed.StartsWith("```"))
                trimmed = trimmed[3..];

            if (trimmed.EndsWith("```"))
                trimmed = trimmed[..^3];

            return trimmed.Trim();
        }

        private static GptAssessment NormalizeAssessment(GptAssessment a)
        {
            int trendQuality = Math.Clamp((int)Math.Round((double)a.TrendQuality), 0, 100);
            int flowQuality = Math.Clamp((int)Math.Round((double)a.FlowQuality), 0, 100);
            int regimeQuality = Math.Clamp((int)Math.Round((double)a.RegimeQuality), 0, 100);

            // Handle Score as double now
            int score = Math.Clamp((int)Math.Round(a.Score), -100, 100);

            int longConf = a.LongConfidence;
            int shortConf = a.ShortConfidence;

            int totalConf = longConf + shortConf;
            if (totalConf != 100 && totalConf > 0)
            {
                longConf = (int)Math.Round(longConf * 100.0 / totalConf);
                shortConf = 100 - longConf;
            }
            else if (totalConf == 0)
            {
                longConf = 50;
                shortConf = 50;
            }

            return new GptAssessment
            {
                DirectionalBias = a.DirectionalBias,
                LongConfidence = longConf,
                ShortConfidence = shortConf,
                Score = score,
                TrendQuality = trendQuality,
                FlowQuality = flowQuality,
                RegimeQuality = regimeQuality,
                RiskLevel = a.RiskLevel,
                StateAssessment = a.StateAssessment,
                DominantIntent = a.DominantIntent,
                MarketStructure = a.MarketStructure,
                BehaviorEvidence = a.BehaviorEvidence,
                Summary = a.Summary,
                KeyDrivers = a.KeyDrivers,
                Contradictions = a.Contradictions,
                Tradeability = a.Tradeability,
                RecommendedAction = a.RecommendedAction,
            };
        }
    }
}