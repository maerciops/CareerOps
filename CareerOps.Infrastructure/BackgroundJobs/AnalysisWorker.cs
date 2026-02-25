using CareerOps.Application.Interfaces;
using CareerOps.Domain.Enums;
using CareerOps.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CareerOps.Infrastructure.BackgroundJobs;

public class AnalysisWorker : BackgroundService
{
    private readonly IAnalysisQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalysisWorker> _logger;

    public AnalysisWorker(IAnalysisQueue queue, IServiceProvider serviceProvider, ILogger<AnalysisWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 AI Analysis Worker Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. QUEUE LISTENER (Outer Scope)
                // If the app shuts down, this throws OperationCanceledException and gracefully exits.
                var jobId = await _queue.DequeueAnalysisAsync(stoppingToken);

                _logger.LogInformation($"[Worker] Picked up JobId: {jobId}");

                // 2. JOB PROCESSOR (Inner Scope)
                // We isolate the job processing so if Gemini crashes, the worker loop stays alive!
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
                    var pdfParser = scope.ServiceProvider.GetRequiredService<IPdfParserService>();
                    var analysisService = scope.ServiceProvider.GetRequiredService<IAnalysisService>();

                    var job = await jobRepository.GetJobByIdAsync(jobId);
                    if (job == null) continue;

                    if (string.IsNullOrEmpty(job.ResumeURL))
                    {
                        _logger.LogWarning($"[Worker] JobId: {jobId} aborted. ResumeURL is missing.");
                        job.UpdateAnalysisStatus(AnalysisStatus.Failed, "Falha no sistema: O link do currículo foi perdido.");
                        await jobRepository.UpdateJobAsync(job);
                        continue;
                    }

                    job.UpdateAnalysisStatus(AnalysisStatus.Processing);
                    await jobRepository.UpdateJobAsync(job);

                    // --- THE HEAVY LIFTING ---
                    var bytesPdf = await storageService.GetFileBytesAsync(job.ResumeURL);
                    var resumeText = await pdfParser.ExtractTextFromPdfAsync(bytesPdf);
                    var jobDescription = job.Description ?? string.Empty;

                    var aiResult = await analysisService.AnalyzeJobCompatibilityAsync(jobDescription, resumeText);

                    // --- SUCCESS ---
                    job.AiAnalysisResult = aiResult;
                    job.UpdateAnalysisStatus(AnalysisStatus.Completed);
                    await jobRepository.UpdateJobAsync(job);

                    _logger.LogInformation($"[Worker] Successfully completed JobId: {jobId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Worker] AI Analysis failed for JobId: {jobId}");

                    using var failScope = _serviceProvider.CreateScope();
                    var fallbackRepo = failScope.ServiceProvider.GetRequiredService<IJobRepository>();

                    var failedJob = await fallbackRepo.GetJobByIdAsync(jobId);
                    if (failedJob != null)
                    {
                        failedJob.UpdateAnalysisStatus(AnalysisStatus.Failed, "Erro na comunicação com a inteligência artificial. Tente novamente.");
                        await fallbackRepo.UpdateJobAsync(failedJob);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception criticalEx)
            {
                // The queue itself crashed. Extremely rare.
                _logger.LogCritical(criticalEx, "[Worker] Fatal error in the background queue listener.");
            }
        }
    }
}
