using CareerOps.Application.Common;
using CareerOps.Application.DTOs;
using CareerOps.Application.Interfaces;
using CareerOps.Domain.Entities;
using CareerOps.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using System.IO;

namespace CareerOps.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IValidator<JobApplicationRequest> _createValidator;
    private readonly IValidator<UpdateJobRequest> _updateValidator;
    private readonly IAnalysisService _analysisService;
    private readonly IPdfParserService _pdfParserService;

    public JobService(
        IJobRepository jobRepository, 
        ICurrentUserService currentUserService, 
        IMapper mapper, 
        IValidator<JobApplicationRequest> createValidator, 
        IValidator<UpdateJobRequest> updateValidator,
        IAnalysisService analysisService,
        IPdfParserService pdfParserService)
    {
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _analysisService = analysisService;
        _pdfParserService = pdfParserService;
    }

    public async Task<Result<JobApplicationResponse>> AnalyzeJobAsync(Guid id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);

        if (job == null || job.OwnerId != _currentUserService.UserId) return "Job não encontrado.";

        var bytesPdf = await File.ReadAllBytesAsync("C:/Users/nerso/Downloads/CV_Maercio_Software_Engineer.pdf");

        var resumeText = await _pdfParserService.ExtractTextFromPdfAsync(bytesPdf);

        var jobDescription = job.Description ?? string.Empty;

        var aiAnalysisResult = await _analysisService.AnalyzeJobCompatibilityAsync(jobDescription, resumeText);

        job.AiAnalysisResult = aiAnalysisResult;

        await _jobRepository.UpdateJobAsync(job);

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

    private string FormatErrors(FluentValidation.Results.ValidationResult listErrors)
    {
        return string.Join(" | ", listErrors.Errors.Select(e => e.ErrorMessage));
    }
}