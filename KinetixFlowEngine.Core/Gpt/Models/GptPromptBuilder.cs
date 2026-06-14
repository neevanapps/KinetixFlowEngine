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

        public string BuildReviewPrompt(    GptMarketSnapshotV2 snapshot)
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
Return ONLY valid JSON matching the schema.

Arrays are ordered:
[10m,30m,60m]

Interpret trend strengthening/weakening from the relationship between timeframes.

OIChange:
Positive = increasing participation.
Negative = decreasing participation.

FlowImpactEfficiency:
Positive = efficient directional flow.
Negative = absorption / inefficient flow.
Focus on relative differences across timeframes.

LongConfidence + ShortConfidence must equal 100.

Score:
Range -100 to 100.
Positive = bullish.
Negative = bearish.
Magnitude reflects conviction.

Contradictions should contain only material factors that reduce confidence in the primary bias.
Return an empty array if none exist.

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
