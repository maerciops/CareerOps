using System.Text.Json.Serialization;

namespace CareerOps.Infrastructure.Externalservices.Gemini.DTOs;

public class ContentDto
{
    [JsonPropertyName("parts")]
    public List<PartDto>? Parts { get; set; }
}
