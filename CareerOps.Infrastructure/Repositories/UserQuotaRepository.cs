using CareerOps.Application.Interfaces;
using CareerOps.Domain.Entities;
using CareerOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerOps.Infrastructure;

public class UserQuotaRepository : IUserQuotaRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserQuotaRepository(ApplicationDbContext context)
    {
        _dbContext = context;
    }

    public void Add(UserQuota quota)
    {
        _dbContext.Add(quota);
    }

    public async Task<UserQuota?> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _dbContext.UserQuotas.FirstOrDefaultAsync(x => x.OwnerId == ownerId);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
