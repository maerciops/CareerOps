namespace CareerOps.Domain.Enums;

public enum AnalysisStatus
{
    None = 0,        // No resume uploaded yet
    Pending = 1,     // Resume uploaded, waiting for worker
    Processing = 2,  // Worker is currently calling Gemini
    Completed = 3,   // AI analysis finished successfully
    Failed = 4       // Gemini API failed or PDF was unreadable
}
