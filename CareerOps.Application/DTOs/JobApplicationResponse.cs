namespace CareerOps.Application.DTOs;

public class JobApplicationResponse
{
    public Guid Id { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? URL { get; set; }
    public decimal? SalaryRange { get; set; }
    public string? Status { get; set; } 
    public string? ResumeURL { get; set; }
    public string? AiAnalysisResult { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

}
