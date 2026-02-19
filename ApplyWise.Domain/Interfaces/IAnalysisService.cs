namespace ApplyWise.Domain.Interfaces;

public interface IAnalysisService
{
    Task<string> AnalyzeJobCompatibilityAsync(string jobDescription, string resumeText);
}
