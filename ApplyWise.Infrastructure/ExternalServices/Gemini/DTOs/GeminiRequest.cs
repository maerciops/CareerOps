using System.Text.Json.Serialization;

namespace ApplyWise.Infrastructure.Externalservices.Gemini.DTOs;

public class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<ContentDto>? Contents { get; set; }
}
