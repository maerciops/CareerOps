using ApplyWise.Domain.Entities;
using ApplyWise.Domain.Interfaces;
using ApplyWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApplyWise.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _jobContext;

    public JobRepository(ApplicationDbContext context)
    {
        _jobContext = context;
    }

    public Task DeleteJobAsync(Guid id)
    {
        throw new NotImplementedException();
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