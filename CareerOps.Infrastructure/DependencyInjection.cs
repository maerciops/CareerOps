using CareerOps.Domain.Interfaces;
using CareerOps.Infrastructure.Persistence;
using CareerOps.Infrastructure.Repositories;
using CareerOps.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CareerOps.Infrastructure.Externalservices.Gemini.Configuration;
using CareerOps.Infrastructure.Externalservices.Parsers;

namespace CareerOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configurar o SQL Server (DbContext)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

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

        return services;
    }
}
