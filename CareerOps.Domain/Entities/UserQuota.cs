using CareerOps.Domain.Exceptions;

namespace CareerOps.Domain.Entities;

public class UserQuota
{
    public Guid OwnerId { get; private set; }
    public int MaxDailyRequests { get; private set; }
    public int UsedDailyRequests { get; private set; }
    public DateTime LastResetDate { get; private set; }

    protected UserQuota() { }

    public UserQuota(Guid ownerId, int maxDailyRequests)
    {
        OwnerId = ownerId;
        MaxDailyRequests = maxDailyRequests;
        UsedDailyRequests = 0;
        LastResetDate = DateTime.UtcNow.Date;
    }

    public void ConsumeAnalysis()
    {
        if (!CanRequestAnalysis())
        {
            throw new QuotaExceededDomainException("O usuário alcançou o limite máximo de solicitações diárias.");
        }

        if (DateTime.UtcNow.Date > LastResetDate.Date)
        {
            UsedDailyRequests = 0;
            LastResetDate = DateTime.UtcNow.Date;
        }

        UsedDailyRequests++;
    }

    public bool CanRequestAnalysis()
    {
        if (DateTime.UtcNow.Date > LastResetDate.Date) return true;

        if (DateTime.UtcNow.Date == LastResetDate.Date && UsedDailyRequests < MaxDailyRequests) return true;

        return false;
    }
}