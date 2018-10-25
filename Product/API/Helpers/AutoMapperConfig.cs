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
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<Enrollment, EnrollmentDto>().ReverseMap();
            CreateMap<Subscriber, SubscriberDto>().ReverseMap();
            CreateMap<WozCourseEnrollment, WozCourseEnrollmentDto>().ReverseMap();
            CreateMap<PromoCode, PromoCodeDto>()
                .ForMember(x => x.IsValid, opt => opt.Ignore())
                .ForMember(x => x.ValidationMessage, opt => opt.Ignore())
                .ForMember(x => x.Discount, opt => opt.Ignore())
                .ForMember(x => x.FinalCost, opt => opt.Ignore())
                .ForMember(x => x.PromoCodeRedemptionGuid, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}