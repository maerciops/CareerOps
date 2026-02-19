using ApplyWise.Domain.Exceptions;
using ApplyWise.Domain.Interfaces;
using ApplyWise.Infrastructure.Externalservices.Gemini.Configuration;
using ApplyWise.Infrastructure.Externalservices.Gemini.DTOs;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ApplyWise.Infrastructure;

public class GeminiService : IAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiService(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> AnalyzeJobCompatibilityAsync(string jobDescription, string resumeText)
    {
        string cPromptAnalyse = BuildPrompt(jobDescription, resumeText);

        PartDto parts = new PartDto { Text = cPromptAnalyse };

        ContentDto content = new ContentDto()
        {
            Parts = new List<PartDto>() { parts }
        };

        GeminiRequest requestPayload = new GeminiRequest 
        {
            Contents = new List<ContentDto>() { content }

        };

        var body = JsonSerializer.Serialize(requestPayload);

        var fullUrl = $"{_options.UrlBase.TrimEnd('/')}/{_options.ModelEndpoint.TrimStart('/')}";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, fullUrl);

        requestMessage.Headers.Add("x-goog-api-key", _options.GeminiApiKey);

        requestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException($"Erro na API Gemini ({response.StatusCode}): {errorContent}"); 
        }

        var responseDto = await response.Content.ReadFromJsonAsync<GeminiResponse>();

        var result = responseDto?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        if (result == null) throw new ExternalServiceException("Erro na API Gemini: a resposta veio vazia.");

        return result;
    }

    private string BuildPrompt(string job, string resume)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Atue como um algoritmo de ATS rigoroso. Analise a compatibilidade:");
        sb.AppendLine($"[INICIO DA VAGA] {job} [FIM DA VAGA]");
        sb.AppendLine($"[INICIO DO CURRICULO] {resume} [FIM DO CURRICULO]");
        sb.AppendLine("INSTRUCOES: 1. Sem emojis. 2. Direto e objetivo. 3. Considere sinonimos.");
        sb.AppendLine("Gere o relatório seguindo este modelo:");
        sb.AppendLine("RELATORIO DE ANALISE ATS");
        sb.AppendLine("------------------------");
        sb.AppendLine("1. NIVEL DE COMPATIBILIDADE: [0-100]%");
        sb.AppendLine("2. JUSTIFICATIVA: [Frase curta]");
        sb.AppendLine("3. PALAVRAS-CHAVE AUSENTES: [- Itens]");
        sb.AppendLine("4. PONTOS FORTES: [- Itens]");
        sb.AppendLine("5. SUGESTOES: [- Ações]");
        sb.AppendLine("6. VEREDITO FINAL: [APROVADO / REVISAO MANUAL / REJEITADO]");

        return sb.ToString();
    }
}