using ApplyWise.Application.Common;
using ApplyWise.Application.DTOs;
using ApplyWise.Application.Interfaces;
using ApplyWise.Domain.Entities;
using ApplyWise.Domain.Interfaces;
using AutoMapper;
using FluentValidation;

namespace ApplyWise.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IValidator<JobApplicationRequest> _createValidator;
    private readonly IValidator<UpdateJobRequest> _updateValidator;

    public JobService(IJobRepository jobRepository, ICurrentUserService currentUserService, IMapper mapper, IValidator<JobApplicationRequest> createValidator, IValidator<UpdateJobRequest> updateValidator)
    {
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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