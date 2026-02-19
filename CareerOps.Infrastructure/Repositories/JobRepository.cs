using CareerOps.Domain.Entities;
using CareerOps.Domain.Interfaces;
using CareerOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerOps.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _jobContext;

    public JobRepository(ApplicationDbContext context)
    {
        _jobContext = context;
    }

    public async Task DeleteJobAsync(JobApplication job)
    {
        _jobContext.Remove(job);
        await _jobContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetAllJobsAsync(Guid userId)
    {
        return await _jobContext
            .Jobs
            .Where(job => job.OwnerId == userId)
            .ToListAsync();
    }

    public async Task<JobApplication?> GetJobByIdAsync(Guid id)
    {
        return await _jobContext.Jobs.FirstOrDefaultAsync(job => job.Id == id);
    }

    public async Task InsertJobAsync(JobApplication job)
    {
        await _jobContext.Jobs.AddAsync(job);
        await _jobContext.SaveChangesAsync();
    }

    public async Task UpdateJobAsync(JobApplication job)
    {
        await _jobContext.SaveChangesAsync();
    }
}