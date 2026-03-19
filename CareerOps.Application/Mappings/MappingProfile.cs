using AutoMapper;
using CareerOps.Application.DTOs;
using CareerOps.Domain.Entities;
using CareerOps.Domain.Enums;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<JobApplication, JobApplicationResponse>()
            .ForMember(dest => dest.AnalysisStatus, opt => opt.MapFrom(src =>
                src.Analyses
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => (AnalysisStatus?)a.AnalysisStatus)
                    .FirstOrDefault() ?? AnalysisStatus.Pending))

            .ForMember(dest => dest.AiAnalysisResult, opt => opt.MapFrom(src =>
                src.Analyses
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.AiAnalysisResult)
                    .FirstOrDefault()))

            .ForMember(dest => dest.AnalysisErrorMessage, opt => opt.MapFrom(src =>
                src.Analyses
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.AnalysisErrorMessage)
                    .FirstOrDefault()))

            .ForMember(dest => dest.ResumeURL, opt => opt.MapFrom(src =>
                src.Analyses
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.ResumeUrl)
                    .FirstOrDefault() ?? src.ResumeURL)); // Fallback para a URL original da vaga

        CreateMap<JobAnalysis, JobAnalysis>();
    }
}