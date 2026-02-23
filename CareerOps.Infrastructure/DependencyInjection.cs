using CareerOps.Domain.Interfaces;
using CareerOps.Infrastructure.Auth;
using CareerOps.Infrastructure.Externalservices.Azure;
using CareerOps.Infrastructure.Externalservices.Azure.Configuration;
using CareerOps.Infrastructure.Externalservices.Gemini.Configuration;
using CareerOps.Infrastructure.Externalservices.Parsers;
using CareerOps.Infrastructure.Persistence;
using CareerOps.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CareerOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configurar o SQL Server (DbContext)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.Configure<AzureOptions>(configuration.GetSection(AzureOptions.SectionName));

        services.AddHttpClient<IAnalysisService, GeminiService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;

            if (!string.IsNullOrEmpty(options.UrlBase))
            {
                client.BaseAddress = new Uri(options.UrlBase);
            }

            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();
        services.AddScoped<IPdfParserService, PdfParserService>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();

        return services;
    }
}
