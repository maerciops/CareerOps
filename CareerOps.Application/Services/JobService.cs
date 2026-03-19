using AutoMapper;
using CareerOps.Application.Common;
using CareerOps.Application.DTOs;
using CareerOps.Application.Interfaces;
using CareerOps.Domain.Entities;
using CareerOps.Domain.Enums;
using CareerOps.Domain.Exceptions;
using CareerOps.Domain.Interfaces;
using FluentValidation;

namespace CareerOps.Application.Services;

public class JobService : IJobService
{
    private readonly IUserQuotaRepository _userQuotaRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IValidator<JobApplicationRequest> _createValidator;
    private readonly IValidator<UpdateJobRequest> _updateValidator;
    private readonly IAnalysisService _analysisService;
    private readonly IPdfParserService _pdfParserService;
    private readonly IStorageService _azureStorageService;
    private readonly IAnalysisQueue _analysisQueue;

    public JobService(
        IUserQuotaRepository userQuotaRepository,
        IJobRepository jobRepository, 
        ICurrentUserService currentUserService, 
        IMapper mapper, 
        IValidator<JobApplicationRequest> createValidator, 
        IValidator<UpdateJobRequest> updateValidator,
        IAnalysisService analysisService,
        IPdfParserService pdfParserService,
        IStorageService azureStorageService,
        IAnalysisQueue analysisQueue)
    {
        _userQuotaRepository = userQuotaRepository;
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _analysisService = analysisService;
        _pdfParserService = pdfParserService;
        _azureStorageService = azureStorageService;
        _analysisQueue = analysisQueue;
    }

    public async Task<Result<JobApplicationResponse>> AnalyzeJobAsync(Guid id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);

        if (job == null || job.OwnerId != _currentUserService.UserId) return "Job não encontrado.";

        if (string.IsNullOrEmpty(job.ResumeURL)) return "Nenhum currículo anexado a esta vaga.";

        var quotaResult = await _userQuotaRepository.ConsumeQuotaAsync(_currentUserService.UserId);

        if (!quotaResult.IsSuccess) return quotaResult.Error!;

        var analysis = await _jobRepository.AddAnalysisAsync(job.Id, job.ResumeURL);
        await _analysisQueue.QueueAnalysisAsync(analysis.Id);

        return Result<JobApplicationResponse>.Success(_mapper.Map<JobApplicationResponse>(job));
    }

    public async Task<Result<JobApplicationResponse>> CreateJobAsync(JobApplicationRequest request)
    {
        var validated = await _createValidator.ValidateAsync(request);

        if (!validated.IsValid) return FormatErrors(validated);

        var jobApplication = _mapper.Map<JobApplication>(request, opt => opt.Items["UserId"] = _currentUserService.UserId);

        await _jobRepository.InsertJobAsync(jobApplication);

        var response = _mapper.Map<JobApplicationResponse>(jobApplication);

        return Result<JobApplicationResponse>.Success(response);
    }

    public async Task<Result> DeleteJobAsync(Guid id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);

        if (job == null || job.OwnerId != _currentUserService.UserId) return "Job não encontrado.";

        await _jobRepository.DeleteJobAsync(job);
        
        return Result.Success();
    }

    public async Task<Result<IEnumerable<JobApplicationResponse>>> GetAllJobsAsync()
    {
        // O repositório já deve retornar a Job com as Analyses incluídas (Eager Loading interno)
        var jobs = await _jobRepository.GetAllJobsAsync(_currentUserService.UserId);
        return Result<IEnumerable<JobApplicationResponse>>.Success(_mapper.Map<IEnumerable<JobApplicationResponse>>(jobs));
    }

    public async Task<Result<JobApplicationResponse>> GetJobByIdAsync(Guid id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);

        if (job == null) return "Job não encontrado.";

        var response = _mapper.Map<JobApplicationResponse>(job);

        return Result<JobApplicationResponse>.Success(response);
    }

    public async Task<Result<JobApplicationResponse>> UpdateJobAsync(Guid id, UpdateJobRequest request)
    {
        var validated = await _updateValidator.ValidateAsync(request);

        if (!validated.IsValid) return FormatErrors(validated);

        var job = await _jobRepository.GetJobByIdAsync(id);

        if (job == null || job.OwnerId != _currentUserService.UserId) return "Job não encontrado.";

        _mapper.Map(request, job);

        await _jobRepository.UpdateJobAsync(job);

        var response = _mapper.Map<JobApplicationResponse>(job);

        return Result<JobApplicationResponse>.Success(response);
    }

    public async Task<Result<JobApplicationResponse>> UploadResumeAsync(Guid jobId, Stream fileStream, string fileName, string contentType = "application/pdf")
    {
        var job = await _jobRepository.GetJobByIdAsync(jobId);
        if (job == null) return "Job não encontrado.";

        var newResumeUrl = await _azureStorageService.UploadFileAsync(fileStream, fileName, contentType);

        var newAnalysisId = await _jobRepository.UpdateResumeAndAddAnalysisAsync(job.Id, newResumeUrl);

        if (newAnalysisId != Guid.Empty)
        {
            await _analysisQueue.QueueAnalysisAsync(newAnalysisId);
        }

        job.ResumeURL = newResumeUrl;

        return Result<JobApplicationResponse>.Success(_mapper.Map<JobApplicationResponse>(job));
    }

    private string FormatErrors(FluentValidation.Results.ValidationResult listErrors)
    {
        return string.Join(" | ", listErrors.Errors.Select(e => e.ErrorMessage));
    }

    private async Task<Result> HandleQuotaAsync()
    {
        var userId = _currentUserService.UserId;
        var quota = await _userQuotaRepository.GetByOwnerIdAsync(userId);

        if (quota == null)
        {
            quota = new UserQuota(userId, 5); // Cria a quota padrão
            _userQuotaRepository.Add(quota);
        }

        try
        {
            // A própria entidade valida e lança QuotaExceededDomainException se falhar
            quota.ConsumeAnalysis();

            await _userQuotaRepository.SaveChangesAsync();
            return Result.Success();
        }
        catch (QuotaExceededDomainException ex)
        {
            // Captura a regra de negócio da entidade e retorna como falha amigável
            return Result.Failure(ex.Message);
        }
    }
}