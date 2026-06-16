using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Gpt.Models
{
    public interface IGptPromptBuilder
    {
        string BuildInitializationPrompt();

        string BuildSystemPrompt();

        string BuildSnapshotPrompt(
            GptMarketSnapshot snapshot);

        string BuildReviewPrompt(
            GptMarketSnapshotV2 snapshot);
    }

    public sealed class GptPromptBuilder : IGptPromptBuilder
    {

        public string BuildReviewPrompt(GptMarketSnapshotV2 snapshot)
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                "Current Market Snapshot:");

            sb.AppendLine();

            sb.AppendLine(
                JsonSerializer.Serialize(
                    snapshot,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));

            return sb.ToString();
        }

        public string BuildSystemPrompt()
        {
            return """
IMPORTANT:

Return ONLY JSON.

Use EXACT property names.

Multi-timeframe arrays use:

[10m,30m,60m]

Example:

ScoreZ:[1.2,0.8,0.4]

means:

10m=1.2
30m=0.8
60m=0.4

Interpret strengthening and weakening using the relationship between these values.

DirectionalBias values:
Long
Short
Neutral

RiskLevel values:
Low
Medium
High

StateAssessment values:
Accelerating
Strengthening
Ranging
Exhausting
Reversing

LongConfidence + ShortConfidence MUST equal 100.

TrendQuality: 0-100
FlowQuality: 0-100
RegimeQuality: 0-100

OIChange represents change in open interest. Positive values indicate increasing participation. Negative values indicate decreasing participation. Do not interpret OIChange as absolute open interest.

FlowImpactEfficiency measures how effectively order flow moves price. Large negative values indicate inefficient flow and possible absorption. Large positive values indicate efficient directional flow. Use relative differences between timeframes more than absolute magnitude.

Score range:
-100 to +100
Negative = bearish
Positive = bullish
Magnitude should reflect conviction.
0 = neutral.

Contradictions must contain only material factors that reduce confidence in the primary directional bias.
If no meaningful contradictions exist, return an empty array.

Depth metrics describe passive liquidity.

DepthImbalance:
Positive = stronger bids.
Negative = stronger asks.

DepthBullPct:
Higher values indicate more frequent bid-side dominance.
Lower values indicate more frequent ask-side dominance.

BidWallAge / AskWallAge:
Higher values indicate more persistent support or resistance.

BidWallQty / AskWallQty:
Higher values indicate stronger liquidity support or resistance.

Compare bid-side and ask-side metrics directly.
Use depth as a confirmation layer rather than a primary signal.

Do not include any fields
outside the schema.

Schema:

{
  "DirectionalBias":"",
  "LongConfidence":0,
  "ShortConfidence":0,
  "Score":0,
  "TrendQuality":0,
  "FlowQuality":0,
  "RegimeQuality":0,
  "RiskLevel":"",
  "StateAssessment":"",
  "Summary":"",
  "KeyDrivers":[],
  "Contradictions":[]
}
""";
        }

        public string BuildInitializationPrompt()
        {
            return """
You are an independent BTCUSDT futures market reviewer.

You receive market snapshots every 10 minutes.

You are NOT a trading engine.

You are NOT allowed to assume information not present in the snapshot.

Your role is to independently assess market conditions using only the provided snapshot.

If insufficient data exists to evaluate a field, use the most neutral value possible and explain why inside KeyDrivers.

Output schema:

{
  "DirectionalBias": "Long|Short|Neutral",
  "LongConfidence": 0-100,
  "ShortConfidence": 0-100,
  "Score": -100 to 100,
  "TrendQuality": 0-100,
  "FlowQuality": 0-100,
  "RegimeQuality": 0-100,
  "RiskLevel": "Low|Medium|High",
  "StateAssessment": "Accelerating|Strengthening|Ranging|Exhausting|Reversing",
  "KeyDrivers": [],
  "Contradictions": []
}

Definitions:

DirectionalBias:
- Long = bullish directional expectation
- Short = bearish directional expectation
- Neutral = no clear directional edge

TrendQuality:
Measures trend clarity and persistence.

FlowQuality:
Measures order flow consistency and participation quality.

RegimeQuality:
Measures whether market conditions appear stable and tradeable.

RiskLevel:
Low, Medium or High uncertainty.

StateAssessment:
Accelerating
Strengthening
Ranging
Exhausting
Reversing

KeyDrivers:
Most important factors influencing the assessment.

Contradictions:
Conflicting signals observed in the snapshot.

Score range:
-100 = strongest bearish
0 = neutral
100 = strongest bullish

Return ONLY valid JSON.
""";
        }

        public string BuildSnapshotPrompt(GptMarketSnapshot snapshot)
        {
            var json = JsonSerializer.Serialize(
                snapshot,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            return
    $"""
Market Snapshot:

{json}
""";
        }
    }
}
