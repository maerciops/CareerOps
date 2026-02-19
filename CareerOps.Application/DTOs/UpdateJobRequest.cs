namespace CareerOps.Application.DTOs;

public class UpdateJobRequest
{
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? URL { get; set; }
    public string? ResumeURL { get; set; }
    public decimal? SalaryRange { get; set; }
}
