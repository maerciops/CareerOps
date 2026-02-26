using CareerOps.Application.DTOs;
using CareerOps.Frontend.Models;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

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
}