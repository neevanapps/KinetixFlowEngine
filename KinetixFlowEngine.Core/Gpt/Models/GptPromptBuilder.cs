using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Gpt.Models
{
    public interface IGptPromptBuilder
    {
        string BuildSystemPrompt();

        string BuildReviewPrompt(GptMarketSnapshotV2 snapshot);
    }

    public sealed class GptPromptBuilder : IGptPromptBuilder
    {
        public string BuildReviewPrompt(GptMarketSnapshotV2 snapshot)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Current Market Snapshot:");
            sb.AppendLine();
            sb.AppendLine(JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
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
10m=1.2, 30m=0.8, 60m=0.4

Interpret strengthening and weakening using the relationship between these values.

DirectionalBias values:
Long, Short, Neutral

RiskLevel values:
Low, Medium, High

StateAssessment values:
Accelerating, Strengthening, Ranging, Exhausting, Reversing

LongConfidence + ShortConfidence MUST equal 100.

TrendQuality, FlowQuality, RegimeQuality: 0-100

OIChange represents change in open interest. Positive = increasing participation. Negative = decreasing participation.

FlowImpactEfficiency: Large negative values indicate absorption / inefficient flow. Large positive values indicate efficient directional flow.

Score range: -100 to +100. Negative = bearish, Positive = bullish. 0 = neutral.

Contradictions must contain only material factors that reduce confidence in the primary directional bias. Return empty array if none exist.

Depth metrics describe passive liquidity. Use them as a confirmation layer, not as the primary signal.

DepthImbalance: Positive = stronger bids, Negative = stronger asks.
DepthBullPct: Higher = more frequent bid-side dominance.
BidWallAge / AskWallAge: Higher = more persistent liquidity.
BidWallQty / AskWallQty: Higher = stronger liquidity walls.

================================================================================
PARTICIPANT BEHAVIOR ANALYSIS (NEW)
================================================================================

You must analyze what different groups of market participants appear to be attempting.

Add these two fields:
- DominantIntent
- BehaviorEvidence

DominantIntent allowed values (choose only one):
- Accumulation          → Smart money / large buyers accumulating quietly
- Distribution          → Large sellers distributing into strength
- Absorption            → One side absorbing aggressive flow (defensive)
- AggressiveBuying      → Strong, efficient buying pressure
- AggressiveSelling     → Strong, efficient selling pressure
- DefensiveLiquidity    → Market makers / liquidity providers defending levels
- SpoofingRisk          → Signs of fake liquidity or manipulation
- RangeBound            → Participants keeping price in a range
- Exhaustion            → One side showing signs of fatigue
- Unclear               → No clear dominant behavior

Rules for DominantIntent:
- Only infer intent when multiple metrics converge and support it.
- If signals are mixed or weak, use "Unclear".
- Always explain your reasoning in BehaviorEvidence using specific data points.
- Do not guess. Stay evidence-based.

BehaviorEvidence:
- List the key data points that support your DominantIntent.
- Be specific (e.g., "Strong negative FlowImpactEfficiency + persistent Ask walls on 30m/60m").

Do not include any fields outside the schema.

================================================================================
SCHEMA
================================================================================

{
  "DirectionalBias": "",
  "LongConfidence": 0,
  "ShortConfidence": 0,
  "Score": 0,
  "TrendQuality": 0,
  "FlowQuality": 0,
  "RegimeQuality": 0,
  "RiskLevel": "",
  "StateAssessment": "",
  "DominantIntent": "",
  "BehaviorEvidence": [],
  "Summary": "",
  "KeyDrivers": [],
  "Contradictions": []
}
""";
        }
    }
}