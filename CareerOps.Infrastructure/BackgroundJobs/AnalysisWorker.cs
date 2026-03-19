using CareerOps.Application.Interfaces;
using CareerOps.Domain.Enums;
using CareerOps.Domain.Interfaces;
using CareerOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace CareerOps.Infrastructure.BackgroundJobs;

public class AnalysisWorker : BackgroundService
{
    private readonly IAnalysisQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalysisWorker> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    public AnalysisWorker(IAnalysisQueue queue, IServiceProvider serviceProvider, ILogger<AnalysisWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"[Polly] Tentativa {retryCount} falhou. Erro: {exception.Message}. Retentando em {timeSpan.TotalSeconds}s...");
                });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 AI Analysis Worker Started (1:N Resilience Mode).");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var analysisId = await _queue.DequeueAnalysisAsync(stoppingToken);
                _logger.LogInformation($"[Worker] Processing AnalysisId: {analysisId}");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
                var pdfParser = scope.ServiceProvider.GetRequiredService<IPdfParserService>();
                var analysisService = scope.ServiceProvider.GetRequiredService<IAnalysisService>();

                var analysis = await context.JobAnalysis
                    .IgnoreQueryFilters()
                    .Include(a => a.JobApplication)
                    .FirstOrDefaultAsync(a => a.Id == analysisId, stoppingToken);

                if (analysis == null)
                {
                    _logger.LogWarning($"[Worker] Analysis {analysisId} not found in database.");
                    continue;
                }

                try
                {
                    analysis.UpdateAnalysisStatus(AnalysisStatus.Processing);
                    await context.SaveChangesAsync(stoppingToken);

                    var finalAiResult = await _retryPolicy.ExecuteAsync(async () =>
                    {
                        _logger.LogInformation($"[Worker] Attempting AI Analysis for AnalysisId: {analysisId}");

                        var bytesPdf = await storageService.GetFileBytesAsync(analysis.ResumeUrl);
                        var resumeText = await pdfParser.ExtractTextFromPdfAsync(bytesPdf);
                        var jobDescription = analysis.JobApplication?.Description ?? string.Empty;

                        return await analysisService.AnalyzeJobCompatibilityAsync(jobDescription, resumeText);
                    });

                    // --- SUCESSO ---
                    analysis.AiAnalysisResult = finalAiResult;
                    analysis.UpdateAnalysisStatus(AnalysisStatus.Completed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Worker] Final failure for AnalysisId: {analysisId}");

                    var friendlyError = MapToFriendlyError(ex.Message);

                    analysis.UpdateAnalysisStatus(AnalysisStatus.Failed, friendlyError);
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception criticalEx)
            {
                _logger.LogCritical(criticalEx, "[Worker] Fatal error in loop listener.");
            }
        }
    }

    private string MapToFriendlyError(string rawError)
    {
        if (string.IsNullOrWhiteSpace(rawError))
            return "Ocorreu um erro inesperado ao processar a análise.";

        return rawError switch
        {
            var e when e.Contains("API_KEY_INVALID") => "Falha na autenticação com o provedor de IA. Por favor, verifique as configurações do sistema.",
            var e when e.Contains("429") || e.Contains("QUOTA_EXCEEDED") => "O limite de requisições da IA foi atingido. Tente novamente em alguns minutos.",
            var e when e.Contains("SAFETY") => "O conteúdo do arquivo foi bloqueado pelos filtros de segurança da IA.",
            var e when e.Contains("BadRequest") || e.Contains("400") => "Houve um problema com os dados enviados para análise (descrição ou currículo inválidos).",
            var e when e.Contains("ServiceUnavailable") || e.Contains("503") => "O serviço de IA está temporariamente indisponível. Estamos tentando restabelecer.",
            _ => "Não foi possível completar a análise após múltiplas tentativas. Verifique seu currículo e tente novamente."
        };
    }
}
