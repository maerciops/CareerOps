using CareerOps.Application.DTOs;
using CareerOps.Application.Common;

namespace CareerOps.Application.Interfaces;

public interface IJobService
{
    Task<Result<JobApplicationResponse>> CreateJobAsync(JobApplicationRequest request);
    Task<Result<JobApplicationResponse>> GetJobByIdAsync(Guid id);
    Task<Result<IEnumerable<JobApplicationResponse>>> GetAllJobsAsync();
    Task<Result<JobApplicationResponse>> UpdateJobAsync(Guid id, UpdateJobRequest request);
    Task<Result> DeleteJobAsync(Guid id);
    Task<Result<JobApplicationResponse>> AnalyzeJobAsync(Guid id);
}
