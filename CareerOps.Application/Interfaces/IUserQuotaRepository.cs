using CareerOps.Application.Common;
using CareerOps.Domain.Entities;

namespace CareerOps.Application.Interfaces;

public interface IUserQuotaRepository
{
    Task<UserQuota?> GetByOwnerIdAsync(Guid ownerId);
    Task<Result> ConsumeQuotaAsync(Guid userId, int defaultMaxRequests = 5);
    void Add(UserQuota quota);
    Task SaveChangesAsync();
}