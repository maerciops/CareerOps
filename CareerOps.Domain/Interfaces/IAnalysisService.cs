namespace CareerOps.Domain.Interfaces;

public interface IAnalysisService
{
    Task<string> AnalyzeJobCompatibilityAsync(string jobDescription, string resumeText);
}
