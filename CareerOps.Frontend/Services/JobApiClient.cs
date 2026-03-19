using CareerOps.Application.Common;
using CareerOps.Application.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace CareerOps.Frontend.Services;

public class JobApiClient
{
    private readonly HttpClient _httpClient;

    public JobApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<JobApplicationResponse>> GetJobsAsync()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Essencial se o JSON for 'company' e o C# 'Company'
        };
        var response = await _httpClient.GetAsync("api/jobs");

        if (response.IsSuccessStatusCode)
        {
            // Usamos as opções de deserialização aqui
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<JobApplicationResponse>>(options);
            return result ?? new List<JobApplicationResponse>();
        }

        return new List<JobApplicationResponse>();
    }

    public async Task UpdateJobAsync(Guid id, JobApplicationRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/jobs/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteJobAsync(Guid id)
    {
        Console.WriteLine($"api/jobs/{id}");
        var response = await _httpClient.DeleteAsync($"api/jobs/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<Result<JobApplicationResponse>> UploadResumeAsync(Guid jobId, Stream fileStream, string fileName)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync($"api/jobs/{jobId}/reanalyze", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JobApplicationResponse>();
                return Result<JobApplicationResponse>.Success(result!);
            }

            var error = await response.Content.ReadAsStringAsync();
            return Result<JobApplicationResponse>.Failure(error ?? "Erro ao processar re-análise.");
        }
        catch (Exception ex)
        {
            return Result<JobApplicationResponse>.Failure($"Erro de conexão: {ex.Message}");
        }
    }
}