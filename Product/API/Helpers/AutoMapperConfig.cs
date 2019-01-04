using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Helpers
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<ApiProfile>();
            });
        }
    }

    public class ApiProfile : Profile
    {
        public ApiProfile()
        {
            CreateMap<Topic, TopicDto>().ReverseMap();
            CreateMap<Vendor, VendorDto>().ReverseMap();
            CreateMap<Enrollment, EnrollmentDto>().ReverseMap();
            CreateMap<WozCourseEnrollment, WozCourseEnrollmentDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<EnrollmentLog, EnrollmentLogDto>().ReverseMap();
            CreateMap<CourseVariantType, CourseVariantTypeDto>().ReverseMap();
            CreateMap<Skill, SkillDto>().ReverseMap();

            // mappings that ignore properties in the Dto that don't exist in the model object
            CreateMap<PromoCode, PromoCodeDto>()
                .ForMember(x => x.IsValid, opt => opt.Ignore())
                .ForMember(x => x.ValidationMessage, opt => opt.Ignore())
                .ForMember(x => x.Discount, opt => opt.Ignore())
                .ForMember(x => x.FinalCost, opt => opt.Ignore())
                .ForMember(x => x.PromoCodeRedemptionGuid, opt => opt.Ignore())
                .ReverseMap();

            // mappings with related entities
            CreateMap<Course, CourseDto>()
                .ForMember(c => c.CourseVariants, opt => opt.MapFrom(src => src.CourseVariants))
                .ForMember(c => c.Vendor, opt => opt.MapFrom(src => src.Vendor))
                .ReverseMap();
            CreateMap<CourseVariant, CourseVariantDto>()
                .ForMember(cv => cv.CourseVariantType, opt => opt.MapFrom(src => src.CourseVariantType))
                .ReverseMap();
            CreateMap<Subscriber, SubscriberDto>()
                .ForMember(s => s.Enrollments, opt => opt.MapFrom(src => src.Enrollments))
                .ForMember(s => s.Skills, opt => opt.MapFrom(src => src.SubscriberSkills.Select(s => s.Skill)))
                .ReverseMap();
            CreateMap<State, StateDto>()
                .ForMember(s => s.Country, opt => opt.MapFrom(src => src.Country))
                .ReverseMap();
        }
    }
}