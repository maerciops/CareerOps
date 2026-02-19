using CareerOps.Domain.Entities;

namespace CareerOps.Domain.Interfaces;

public interface IJobRepository
{
    Task<JobApplication?> GetJobByIdAsync(Guid id);
    Task<IEnumerable<JobApplication>> GetAllJobsAsync(Guid userId);
    Task InsertJobAsync(JobApplication job);
    Task UpdateJobAsync(JobApplication job);
    Task DeleteJobAsync(JobApplication job);
}