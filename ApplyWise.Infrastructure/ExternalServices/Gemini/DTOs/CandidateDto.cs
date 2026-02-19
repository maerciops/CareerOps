using System.Text.Json.Serialization;

namespace ApplyWise.Infrastructure.Externalservices.Gemini.DTOs;

public class CandidateDto
{

    [JsonPropertyName("content")]
    public ContentDto? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }
}
