using System.Text.Json.Serialization;

namespace ApplyWise.Infrastructure.Externalservices.Gemini.DTOs;

public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<CandidateDto>? Candidates { get; set; }
}
