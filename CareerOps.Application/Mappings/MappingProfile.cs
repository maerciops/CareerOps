using CareerOps.Application.DTOs;
using CareerOps.Domain.Entities;
using AutoMapper;

namespace CareerOps.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<JobApplication, JobApplicationResponse>().ReverseMap();
        CreateMap<JobApplication, JobApplicationRequest>()
            .ReverseMap()
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .AfterMap((src, dest, context) =>
            {
                if (context.Items.TryGetValue("UserId", out var userId))
                {
                    dest.OwnerId = (Guid)userId;
                }
            }); ;
        CreateMap<JobApplication, UpdateJobRequest>()
            .ReverseMap()
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore());
    }
}
