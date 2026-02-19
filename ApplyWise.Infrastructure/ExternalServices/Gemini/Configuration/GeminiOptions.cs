namespace ApplyWise.Infrastructure.Externalservices.Gemini.Configuration;

public class GeminiOptions
{
    public const string SectionName = "GeminiSettings";
    public string UrlBase { get; set; } = string.Empty;
    public string ModelEndpoint { get; set; } = string.Empty;
    public string GeminiApiKey { get; set; } = string.Empty;
}
