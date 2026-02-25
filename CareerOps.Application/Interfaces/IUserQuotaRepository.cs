using CareerOps.Domain.Entities;

namespace CareerOps.Application.Interfaces;

public interface IUserQuotaRepository
{
    Task<UserQuota?> GetByOwnerIdAsync(Guid ownerId);
    void Add(UserQuota quota);
    Task SaveChangesAsync();
}