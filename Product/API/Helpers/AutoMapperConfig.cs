using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Helpers;


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
            CreateMap<SubscriberFile, SubscriberFileDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<EducationalInstitution, EducationalInstitutionDto>().ReverseMap();
            CreateMap<EducationalDegree, EducationalDegreeDto>().ReverseMap();
            CreateMap<EducationalDegreeType, EducationalDegreeTypeDto>().ReverseMap();
            CreateMap<CompensationType, CompensationTypeDto>().ReverseMap();
            CreateMap<Campaign, CampaignDto>().ReverseMap();
            CreateMap<CampaignCourseVariant, CampaignCourseVariantDto>().ReverseMap();
            CreateMap<RebateType, RebateTypeDto>().ReverseMap();
            CreateMap<Refund, RefundDto>().ReverseMap();
            CreateMap<Contact, ContactDto>().ReverseMap();
            CreateMap<CampaignStatistic, CampaignStatisticDto>().ReverseMap();
            CreateMap<CampaignDetail, CampaignDetailDto>().ReverseMap();
            CreateMap<v_SubscriberSources, SubscriberSourceDto>().ReverseMap();
            CreateMap<SecurityClearance, SecurityClearanceDto>().ReverseMap();
            CreateMap<EmploymentType, EmploymentTypeDto>().ReverseMap();
            CreateMap<Industry, IndustryDto>().ReverseMap();
            CreateMap<JobPosting, JobPostingDto>().ReverseMap();
            CreateMap<ExperienceLevel, ExperienceLevelDto>().ReverseMap();
            CreateMap<EducationLevel, EducationLevelDto>().ReverseMap();


            // TODO JAB finish this mapping      

            CreateMap<JobViewDto, CloudTalentSolution.MatchingJob>()
              .ForMember(c => c.JobSummary, opt => opt.MapFrom(src => src.JobSummary))
              .ForMember(c => c.JobTitleSnippet, opt => opt.MapFrom(src => src.JobTitleSnippet))
              .ForMember(c => c.SearchTextSnippet, opt => opt.MapFrom(src => src.SearchTextSnippet))          
              .ForPath(c => c.Job.RequisitionId, opt => opt.MapFrom(src => src.JobPostingGuid))
              .ForMember(c => c.SearchTextSnippet, opt => opt.MapFrom(src => src.SearchTextSnippet))
              .ForPath(c => c.Job.PostingExpireTime, opt => opt.MapFrom(src => src.PostingExpirationDateUTC))
              .ForPath(c => c.Job.Name, opt => opt.MapFrom(src => src.CloudTalentUri))
              .ForPath(c => c.Job.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
              .ForPath(c => c.Job.Title, opt => opt.MapFrom(src => src.Title))
              .ForPath(c => c.Job.Description, opt => opt.MapFrom(src => src.Description))
              .ForPath(c => c.Job.PostingPublishTime, opt => opt.MapFrom(src => src.PostingDateUTC))
              .ReverseMap();
            /*
             * 
             *
 TODO JAB Index the following PostingDateUTC,ApplicationDeadlineUTC
 

 
        
 
        
        public string Description { get; set; }
             * 
             * 
             * 
             */





            CreateMap<Contact, EmailContactDto>()
            .ForMember(c => c.email, opt => opt.MapFrom(src => src.Email))
            .ForMember(c => c.last_name, opt => opt.MapFrom(src => src.LastName))
            .ForMember(c => c.first_name, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(c => c.contact_guid, opt => opt.MapFrom(src => src.ContactGuid))
            .ReverseMap();

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
                .ForMember(c => c.Skills, opt => opt.MapFrom(src => src.CourseSkills.Select(cs => cs.Skill)))
                .ReverseMap();
            CreateMap<CourseVariant, CourseVariantDto>()
                .ForMember(cv => cv.CourseVariantType, opt => opt.MapFrom(src => src.CourseVariantType))
                .ReverseMap();
            CreateMap<Subscriber, SubscriberDto>()
                .ForMember(s => s.Enrollments, opt => opt.MapFrom(src => src.Enrollments))
                .ForMember(s => s.Skills, opt => opt.MapFrom(src => src.SubscriberSkills.Select(ss => ss.Skill)))
                .ForMember(s => s.WorkHistory, opt => opt.MapFrom(src => src.SubscriberWorkHistory))
                .ForMember(s => s.EducationHistory, opt => opt.MapFrom(src => src.SubscriberEducationHistory))
                .ForMember(s => s.Files, opt => opt.MapFrom(src => src.SubscriberFile))
                .ReverseMap();
            CreateMap<State, StateDto>()
                .ForMember(s => s.Country, opt => opt.MapFrom(src => src.Country))
                .ReverseMap();

            // dealing with difference between workhistory/workhistorydto and educationhistory/educationhistorydto
            CreateMap<SubscriberWorkHistory, SubscriberWorkHistoryDto>()
                .ForMember(x => x.Company, opt => opt.MapFrom(src => src.Company.CompanyName))
                .ForMember(x => x.CompensationType, opt => opt.MapFrom(src => src.CompensationType.CompensationTypeName));
            CreateMap<SubscriberEducationHistory, SubscriberEducationHistoryDto>()
                .ForMember(x => x.EducationalInstitution, opt => opt.MapFrom(src => src.EducationalInstitution.Name))
                .ForMember(x => x.EducationalDegree, opt => opt.MapFrom(src => src.EducationalDegree.Degree))
                .ForMember(x => x.EducationalDegreeType, opt => opt.MapFrom(src => src.EducationalDegreeType.DegreeType));
        }
    }
}