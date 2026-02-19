using System.Text.Json.Serialization;

namespace CareerOps.Infrastructure.Externalservices.Gemini.DTOs;

public class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<ContentDto>? Contents { get; set; }
}
