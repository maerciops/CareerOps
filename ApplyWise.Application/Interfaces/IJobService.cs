using ApplyWise.Application.DTOs;
using ApplyWise.Application.Common;

namespace ApplyWise.Application.Interfaces;

public interface IJobService
{
    Task<Result<JobApplicationResponse>> CreateJobAsync(JobApplicationRequest request);
    Task<Result<JobApplicationResponse>> GetJobByIdAsync(Guid id);
    Task<Result<IEnumerable<JobApplicationResponse>>> GetAllJobsAsync();
    Task<Result<JobApplicationResponse>> UpdateJobAsync(Guid id, UpdateJobRequest request);
    Task<Result> DeleteJobAsync(Guid id);
}
