using CareerOps.Domain.Common;
using CareerOps.Domain.Enums;

namespace CareerOps.Domain.Entities;

public class JobApplication : BaseEntity
{
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? URL { get; set; }
    public decimal? SalaryRange { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public string? ResumeURL { get; set; }
    public string? AiAnalysisResult { get; set; }

    // Regras de negócio centralizadas e estáticas (carregadas uma única vez na memória)
    private static readonly Dictionary<ApplicationStatus, HashSet<ApplicationStatus>> NextStatusRules =
        new()
        {
            { ApplicationStatus.Applied, new() { ApplicationStatus.Interview, ApplicationStatus.Rejected, ApplicationStatus.Offer } },
            
            { ApplicationStatus.Interview, new() { ApplicationStatus.Offer, ApplicationStatus.Rejected } },
            
            { ApplicationStatus.Rejected, new() },
            { ApplicationStatus.Offer, new() }
        };

    public void UpdateStatus(ApplicationStatus nextStatus)
    {
        if (!NextStatusRules.ContainsKey(Status) || !NextStatusRules[Status].Contains(nextStatus))
        {
            throw new InvalidOperationException($"Transição inválida: Não é permitido mudar de {Status} para {nextStatus}.");
        }

        Status = nextStatus;
        UpdatedAt = DateTime.UtcNow;
    }

}
