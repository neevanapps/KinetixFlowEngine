using KinetixFlowEngine.Core.Gpt.Configuration;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using KinetixFlowEngine.Core.Gpt.Validation;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KinetixFlowEngine.Core.Gpt.Services;

public abstract class CloudReviewServiceBase
    : IModelReviewer
{
    private readonly IGptReviewStore _reviewStore;
    private readonly IGptPromptBuilder _promptBuilder;
    private readonly ChatClient _chatClient;
    private readonly ILogger _logger;

    protected abstract string ModelName { get; }

    public string Name => ModelName;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    protected CloudReviewServiceBase(IGptReviewStore reviewStore, IGptPromptBuilder promptBuilder, IOptions<CloudAiSettings> settings, ILogger logger)
    {
        _reviewStore = reviewStore;
        _promptBuilder = promptBuilder;
        _logger = logger;

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.Value.BaseUrl)
        };

        var client = new OpenAIClient(credential: new System.ClientModel.ApiKeyCredential(settings.Value.ApiKey), options);

        _chatClient = client.GetChatClient(ModelName);

        _logger.LogInformation("BaseUrl:{Url}", settings.Value.BaseUrl);
        _logger.LogInformation("ApiKey Length:{Len}", settings.Value.ApiKey.Length);
    }

    public async Task<GptReviewRecord> ReviewAsync(
        GptMarketSnapshotV2 snapshot,
        CancellationToken ct = default)
    {
        var prompt = _promptBuilder.BuildReviewPrompt(snapshot);
        var systemPrompt = _promptBuilder.BuildSystemPrompt();

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(prompt)
        };

        var rawResponse = string.Empty;
        try
        {
            var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: ct);
            rawResponse = completion?.Value.Content[0].Text;
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(
                ex,
                "Model:{Model} Status:{Status} Message:{Message}",
                ModelName,
                ex.Status,
                ex.Message);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during {ModelName} review for snapshot sequence {snapshot.Sequence}");
        }
        rawResponse = CleanMarkdownWrappers(rawResponse);

        var assessment = JsonSerializer.Deserialize<GptAssessment>(rawResponse, JsonOptions);

        if (assessment == null)
            throw new InvalidOperationException($"Failed to deserialize {ModelName}");

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
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var trimmed = input.Trim();

        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[7..];
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed[3..];
        }

        if (trimmed.EndsWith("```"))
        {
            trimmed = trimmed[..^3];
        }

        return trimmed.Trim();
    }
}