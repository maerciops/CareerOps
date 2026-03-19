using CareerOps.Domain.Common;
using CareerOps.Domain.Entities;
using CareerOps.Domain.Enums;

public class JobAnalysis : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public string ResumeUrl { get; set; } = string.Empty;
    public string? AiAnalysisResult { get; set; } // Centralizado aqui
    public AnalysisStatus AnalysisStatus { get; private set; } = AnalysisStatus.Pending;
    public string? AnalysisErrorMessage { get; private set; }

    public virtual JobApplication JobApplication { get; set; } = null!;

    public void UpdateAnalysisStatus(AnalysisStatus status, string? errorMessage = null)
    {
        AnalysisStatus = status;
        AnalysisErrorMessage = errorMessage;
    }
}