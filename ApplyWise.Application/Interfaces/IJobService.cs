using ApplyWise.Application.DTOs;
using ApplyWise.Domain.Entities;

namespace ApplyWise.Application.Interfaces;

public interface IJobService
{
    Task<Guid> CreateJobAsync(JobApplicationRequest request);
    Task<JobApplication?> GetJobByIdAsync(Guid id);
    Task<IEnumerable<JobApplicationResponse>> GetAllJobsAsync();
    Task UpdateJobAsync(Guid id, UpdateJobRequest request);
    Task DeleteJobAsync(Guid id);
}
