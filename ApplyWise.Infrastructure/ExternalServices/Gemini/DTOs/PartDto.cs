using System.Text.Json.Serialization;

namespace ApplyWise.Infrastructure.Externalservices.Gemini.DTOs;

public class PartDto
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
