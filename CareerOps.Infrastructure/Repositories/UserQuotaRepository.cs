using CareerOps.Application.Interfaces;
using CareerOps.Application.Common;
using CareerOps.Domain.Entities;
using CareerOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerOps.Infrastructure.Repositories;

public class UserQuotaRepository : IUserQuotaRepository
{
    private readonly ApplicationDbContext _context;

    public UserQuotaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserQuota?> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.OwnerId == ownerId);
    }

    // Implementação do método atômico para o Service
    public async Task<Result> ConsumeQuotaAsync(Guid userId, int defaultMaxRequests = 5)
    {
        var quota = await GetByOwnerIdAsync(userId);

        if (quota == null)
        {
            quota = new UserQuota(userId, defaultMaxRequests);
            _context.UserQuotas.Add(quota);
        }

        // Validação da regra de negócio (Domain logic)
        if (!quota.CanRequestAnalysis())
        {
            return Result.Failure("Limite de cota diária atingido.");
        }

        quota.ConsumeAnalysis();

        // Persistência encapsulada
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    // Métodos legados da interface (se ainda precisar mantê-los)
    public void Add(UserQuota quota) => _context.UserQuotas.Add(quota);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}