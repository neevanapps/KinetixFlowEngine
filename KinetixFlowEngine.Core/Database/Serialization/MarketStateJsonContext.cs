using KinetixFlowEngine.Core.Domain.Market;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KinetixFlowEngine.Core.Database.Serialization
{
    [JsonSourceGenerationOptions(WriteIndented = false, DefaultIgnoreCondition = JsonIgnoreCondition.Never, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = new[]
    {
        typeof(JsonStringEnumConverter)
    })]
    [JsonSerializable(typeof(MarketState))]
    public partial class MarketStateJsonContext
    : JsonSerializerContext
    {
    }

    public interface IJsonSerializer<T>
    {
        string Serialize(T value);

        T Deserialize(string payload);
    }

    public sealed class MarketStateSerializer : IJsonSerializer<MarketState>
    {
        public string Serialize(
            MarketState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            return JsonSerializer.Serialize(
                state,
                MarketStateJsonContext.Default.MarketState);
        }

        public MarketState Deserialize(
            string payload)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);

            return JsonSerializer.Deserialize(
                       payload,
                       MarketStateJsonContext.Default.MarketState)
                   ?? throw new InvalidOperationException(
                       "Unable to deserialize MarketState.");
        }
    }
}
