using CareerOps.Application.Common;
using CareerOps.Application.DTOs;
using CareerOps.Application.Interfaces;
using CareerOps.Domain.Entities;
using CareerOps.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using CareerOps.Domain.Enums;

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
        if (string.IsNullOrEmpty(job.ResumeURL)) return "Nenhum currículo foi anexado a esta vaga. Faça o upload antes de analisar.";

        var userId = _currentUserService.UserId;
        var quota = await _userQuotaRepository.GetByOwnerIdAsync(userId);
        if (quota == null)
        {
            quota = new UserQuota(userId, 5);
            _userQuotaRepository.Add(quota);
        }
        quota.ConsumeAnalysis();

        job.UpdateAnalysisStatus(AnalysisStatus.Pending);

        await _userQuotaRepository.SaveChangesAsync();
        await _jobRepository.UpdateJobAsync(job);

        await _analysisQueue.QueueAnalysisAsync(job.Id);

        var response = _mapper.Map<JobApplicationResponse>(job);
        return Result<JobApplicationResponse>.Success(response);
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
        var jobs = await _jobRepository.GetAllJobsAsync(_currentUserService.UserId);

        var response = _mapper.Map<IEnumerable<JobApplicationResponse>>(jobs);

        return Result<IEnumerable<JobApplicationResponse>>.Success(response);
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

        if (job == null || job.OwnerId != _currentUserService.UserId) return "Job não encontrado.";

        var resumeUrl = await _azureStorageService.UploadFileAsync(fileStream, fileName, contentType);

        job.ResumeURL = resumeUrl;

        await _jobRepository.UpdateJobAsync(job);

        var response = _mapper.Map<JobApplicationResponse>(job);

        return Result<JobApplicationResponse>.Success(response);
    }

    private string FormatErrors(FluentValidation.Results.ValidationResult listErrors)
    {
        return string.Join(" | ", listErrors.Errors.Select(e => e.ErrorMessage));
    }
}