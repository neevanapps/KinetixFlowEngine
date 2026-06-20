using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Validation;

public static class GptAssessmentValidator
{
    public static void Validate(
        GptAssessment assessment)
    {
        //if (!Enum.IsDefined(
        //        typeof(DirectionalBias),
        //        assessment.DirectionalBias))
        //{
        //    throw new InvalidOperationException(
        //        "Invalid DirectionalBias.");
        //}

        //if (!Enum.IsDefined(
        //        typeof(RiskLevel),
        //        assessment.RiskLevel))
        //{
        //    throw new InvalidOperationException(
        //        "Invalid RiskLevel.");
        //}

        //if (!Enum.IsDefined(
        //        typeof(StateAssessment),
        //        assessment.StateAssessment))
        //{
        //    throw new InvalidOperationException(
        //        "Invalid StateAssessment.");
        //}

        if (assessment.LongConfidence < 0 ||
            assessment.LongConfidence > 100)
        {
            throw new InvalidOperationException(
                "Invalid LongConfidence.");
        }

        if (assessment.ShortConfidence < 0 ||
            assessment.ShortConfidence > 100)
        {
            throw new InvalidOperationException(
                "Invalid ShortConfidence.");
        }

        if ((assessment.LongConfidence +
             assessment.ShortConfidence) != 100)
        {
            throw new InvalidOperationException(
                "Confidence values must total 100.");
        }

        if (assessment.Score < -100 ||
            assessment.Score > 100)
        {
            throw new InvalidOperationException(
                "Score out of range.");
        }
    }
}