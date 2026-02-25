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
    public ApplicationStatus ApplicationStatus { get; set; } = ApplicationStatus.Applied;
    public string? ResumeURL { get; set; }
    public string? AiAnalysisResult { get; set; }
    public AnalysisStatus AnalysisStatus { get; private set; } = AnalysisStatus.None;
    public string? AnalysisErrorMessage { get; private set; }

    // Regras de negócio centralizadas e estáticas (carregadas uma única vez na memória)
    private static readonly Dictionary<ApplicationStatus, HashSet<ApplicationStatus>> NextStatusRules =
        new()
        {
            { ApplicationStatus.Applied, new() { ApplicationStatus.HrInterview, ApplicationStatus.TechnicalInterview, ApplicationStatus.Rejected, ApplicationStatus.Offer } },
            
            { ApplicationStatus.HrInterview, new() { ApplicationStatus.TechnicalInterview, ApplicationStatus.Offer, ApplicationStatus.Rejected } },

            { ApplicationStatus.TechnicalInterview, new() { ApplicationStatus.Offer, ApplicationStatus.Rejected } },

            { ApplicationStatus.Rejected, new() },
            { ApplicationStatus.Offer, new() }
        };

    public void UpdateStatus(ApplicationStatus nextStatus)
    {
        if (!NextStatusRules.ContainsKey(ApplicationStatus) || !NextStatusRules[ApplicationStatus].Contains(nextStatus))
        {
            throw new InvalidOperationException($"Transição inválida: Não é permitido mudar de {ApplicationStatus} para {nextStatus}.");
        }

        ApplicationStatus = nextStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAnalysisStatus(AnalysisStatus status, string? errorMessage = null)
    {
        AnalysisStatus = status;
        AnalysisErrorMessage = errorMessage;
    }

}
