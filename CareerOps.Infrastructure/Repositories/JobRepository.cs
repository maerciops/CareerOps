using CareerOps.Domain.Entities;
using CareerOps.Domain.Enums;
using CareerOps.Domain.Interfaces;
using CareerOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace CareerOps.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _jobContext;

    public JobRepository(ApplicationDbContext context)
    {
        _jobContext = context;
    }

    public async Task<JobAnalysis> AddAnalysisAsync(Guid jobId, string resumeUrl)
    {
        var analysis = new JobAnalysis
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobId,
            ResumeUrl = resumeUrl,
            // AnalysisStatus nasce como Pending por padrão no construtor/entidade
        };

        await _jobContext.JobAnalysis.AddAsync(analysis);
        await _jobContext.SaveChangesAsync();

        return analysis;
    }

    public async Task DeleteJobAsync(JobApplication job)
    {
        _jobContext.Entry(job).Property("IsDeleted").CurrentValue = true;
        await _jobContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetAllJobsAsync(Guid userId)
    {
        return await _jobContext.Jobs
            .Include(j => j.Analyses)
            .ToListAsync();
    }

    public async Task<JobApplication?> GetJobByIdAsync(Guid id)
    {
        return await _jobContext.Jobs
        .Include(j => j.Analyses) // <--- ESSA LINHA RESOLVE O NULL
        .FirstOrDefaultAsync(j => j.Id == id);
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

    public async Task<Guid> UpdateResumeAndAddAnalysisAsync(Guid jobId, string newResumeUrl)
    {
        var job = await _jobContext.Jobs.FindAsync(jobId);
        if (job == null) return Guid.Empty;

        job.ResumeURL = newResumeUrl;

        var analysis = new JobAnalysis
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobId,
            ResumeUrl = newResumeUrl
        };

        _jobContext.Jobs.Update(job);
        await _jobContext.JobAnalysis.AddAsync(analysis);
        await _jobContext.SaveChangesAsync();

        return analysis.Id;
    }
}