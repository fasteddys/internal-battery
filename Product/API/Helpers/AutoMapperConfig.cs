using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using UpDiddyApi.Models;
using UpDiddyApi.Models.Views;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Helpers;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Domain.Models;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Domain;
using UpDiddyLib.Domain.Models.Reports;

using UpDiddyLib.Domain.AzureSearch;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models.G2;

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
            CreateMap<User, CreateUserDto>()
                .ForMember(cud => cud.FirstName, u => u.Ignore())
                .ForMember(cud => cud.JobReferralCode, u => u.Ignore())
                .ForMember(cud => cud.LastName, u => u.Ignore())
                .ForMember(cud => cud.PartnerGuid, u => u.Ignore())
                .ForMember(cud => cud.PhoneNumber, u => u.Ignore())
                .ForMember(cud => cud.ReferrerUrl, u => u.Ignore())
                .ForMember(cud => cud.SubscriberGuid, u => u.Ignore())
                .ReverseMap();

            CreateMap<Topic, UpDiddyLib.Dto.TopicDto>().ReverseMap();
            CreateMap<Topic, UpDiddyLib.Domain.Models.TopicDto>().ReverseMap();
            CreateMap<List<UpDiddyLib.Domain.Models.TopicDto>, TopicListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
                .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<Vendor, VendorDto>().ReverseMap();
            CreateMap<Enrollment, EnrollmentDto>().ReverseMap();
            CreateMap<WozCourseEnrollment, WozCourseEnrollmentDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<CountryDetailDto, Country>().ReverseMap();

            CreateMap<List<CountryDetailDto>, CountryDetailListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.Countries, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<StateDetailDto, State>().ReverseMap();
            CreateMap<List<StateDetailDto>, StateDetailListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.States, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<EnrollmentLog, EnrollmentLogDto>().ReverseMap();
            CreateMap<CourseVariantType, CourseVariantTypeDto>().ReverseMap();
            CreateMap<Skill, UpDiddyLib.Dto.SkillDto>().ReverseMap();
            CreateMap<Company, CompanyDto>()
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<CompanyDto>, CompanyListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.Companies, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<List<CourseFavoriteDto>, CourseFavoriteListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.CourseFavorites, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<EducationalInstitution, EducationalInstitutionDto>().ReverseMap();
            CreateMap<EducationalDegree, EducationalDegreeDto>().ReverseMap();
            CreateMap<EducationalDegreeType, UpDiddyLib.Dto.EducationalDegreeTypeDto>().ReverseMap();
            CreateMap<EducationalDegreeType, UpDiddyLib.Domain.Models.EducationalDegreeTypeDto>().ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.EducationalDegreeTypeDto>, EducationalDegreeTypeListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.EducationalDegreeTypes, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<CompensationType, UpDiddyLib.Dto.CompensationTypeDto>().ReverseMap();
            CreateMap<CompensationType, UpDiddyLib.Domain.Models.CompensationTypeDto>()
            .ForMember(x => x.TotalRecords, opt => opt.Ignore())
            .ReverseMap();
            CreateMap<List<UpDiddyLib.Domain.Models.CompensationTypeDto>, CompensationTypeListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.CompensationTypes, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();
            CreateMap<Campaign, CampaignDto>().ReverseMap();
            CreateMap<CampaignCourseVariant, CampaignCourseVariantDto>().ReverseMap();
            CreateMap<RebateType, RebateTypeDto>().ReverseMap();
            CreateMap<Refund, RefundDto>().ReverseMap();

            CreateMap<CampaignStatistic, CampaignStatisticDto>().ReverseMap();
            CreateMap<CampaignDetail, CampaignDetailDto>().ReverseMap();
            CreateMap<v_SubscriberSources, SubscriberSourceStatisticDto>().ReverseMap();
            CreateMap<SecurityClearance, UpDiddyLib.Dto.SecurityClearanceDto>().ReverseMap();
            CreateMap<EmploymentType, UpDiddyLib.Dto.EmploymentTypeDto>().ReverseMap();
            CreateMap<EmploymentType, UpDiddyLib.Domain.Models.EmploymentTypeDto>()
            .ForMember(x => x.TotalRecords, opt => opt.Ignore())
            .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.EmploymentTypeDto>, EmploymentTypeListDto>()
                        .AfterMap((src, dest) =>
                        {
                            if (src != null && src.Count() > 0)
                                dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                            else
                                dest.TotalRecords = 0;
                        })
                      .ForMember(dest => dest.EmploymentTypes, opt => opt.MapFrom(src => src.ToList()))
                      .ReverseMap();

            CreateMap<Industry, UpDiddyLib.Dto.IndustryDto>().ReverseMap();
            CreateMap<Industry, UpDiddyLib.Domain.Models.IndustryDto>()
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.IndustryDto>, IndustryListDto>()
                        .AfterMap((src, dest) =>
                        {
                            if (src != null && src.Count() > 0)
                                dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                            else
                                dest.TotalRecords = 0;
                        })
                      .ForMember(dest => dest.Industries, opt => opt.MapFrom(src => src.ToList()))
                      .ReverseMap();

            CreateMap<JobPostingSkill, JobPostingSkillDto>().ReverseMap();
            CreateMap<ExperienceLevel, UpDiddyLib.Dto.ExperienceLevelDto>().ReverseMap();
            CreateMap<ExperienceLevel, UpDiddyLib.Domain.Models.ExperienceLevelDto>()
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<List<UpDiddyLib.Domain.Models.ExperienceLevelDto>, ExperienceLevelListDto>()
                        .AfterMap((src, dest) =>
                        {
                            if (src != null && src.Count() > 0)
                                dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                            else
                                dest.TotalRecords = 0;
                        })
                      .ForMember(dest => dest.ExperienceLevels, opt => opt.MapFrom(src => src.ToList()))
                      .ReverseMap();

            CreateMap<EducationLevel, UpDiddyLib.Dto.EducationLevelDto>().ReverseMap();
            CreateMap<EducationLevel, UpDiddyLib.Domain.Models.EducationLevelDto>()
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.EducationLevelDto>, EducationLevelListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
                .ForMember(dest => dest.EducationLevels, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<JobCategory, JobCategoryDto>().ReverseMap();
            CreateMap<JobApplication, JobApplicationDto>().ReverseMap();
            CreateMap<RecruiterCompany, RecruiterCompanyDto>().ReverseMap();
            CreateMap<JobPostingFavorite, JobPostingFavoriteDto>().ReverseMap();
            CreateMap<Recruiter, RecruiterDto>().ReverseMap();
            CreateMap<JobSite, JobSiteDto>().ReverseMap();
            CreateMap<JobSiteScrapeStatistic, UpDiddyLib.Dto.JobSiteScrapeStatisticDto>().ReverseMap();


            CreateMap<List<UpDiddyLib.Domain.Models.JobSiteScrapeStatisticDto>, JobSiteScrapeStatisticsListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
                .ForMember(dest => dest.JobSiteScrapeStatistics, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<ResumeParse, ResumeParseDto>().ReverseMap();
            CreateMap<ResumeParseResult, ResumeParseResultDto>().ReverseMap();
            CreateMap<Subscriber, FailedSubscriberDto>().ReverseMap();
            CreateMap<CourseLevel, CourseLevelDto>().ReverseMap();

            CreateMap<List<CourseLevelDto>, CourseLevelListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.CourseLevels, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<RedemptionStatus, RedemptionStatusDto>().ReverseMap();
            CreateMap<ServiceOffering, ServiceOfferingDto>().ReverseMap();
            CreateMap<ServiceOfferingItem, ServiceOfferingItemDto>().ReverseMap();
            CreateMap<ServiceOfferingOrder, ServiceOfferingOrderDto>().ReverseMap();
            CreateMap<ServiceOfferingPromoCodeRedemption, ServiceOfferingPromoCodeRedemptionDto>().ReverseMap();
            CreateMap<FileDownloadTracker, FileDownloadTrackerDto>().ReverseMap();
            CreateMap<Offer, UpDiddyLib.Domain.Models.OfferDto>()
                .ForMember(c => c.PartnerName, opt => opt.MapFrom(src => src.Partner.Name))
                .ForMember(dest => dest.PartnerLogoUrl, opt => opt.MapFrom(src => src.Partner.LogoUrl))
                .ForMember(dest => dest.PartnerGuid, opt => opt.MapFrom(src => src.Partner.PartnerGuid))
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.OfferDto>, OfferListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
              .ForMember(dest => dest.Offers, opt => opt.MapFrom(src => src.ToList()))
              .ReverseMap();

            CreateMap<SecurityClearance, UpDiddyLib.Domain.Models.SecurityClearanceDto>()
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.SecurityClearanceDto>, SecurityClearanceListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
              .ForMember(dest => dest.SecurityClearances, opt => opt.MapFrom(src => src.ToList()))
              .ReverseMap();
            CreateMap<Partner, UpDiddyLib.Domain.Models.PartnerDto>()
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<List<UpDiddyLib.Domain.Models.PartnerDto>, PartnerListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
                .ForMember(dest => dest.Partners, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<RelatedJobDto, CareerPathJobDto>()
            .ForMember(c => c.CompanyLogoUrl, opt => opt.MapFrom(src => src.LogoUrl)).ReverseMap();
            CreateMap<Traitify, TraitifyDto>().ReverseMap();
            CreateMap<Course, CourseDetailDto>()
                .ForMember(c => c.VendorLogoUrl, opt => opt.MapFrom(src => src.Vendor.LogoUrl))
                .ForMember(c => c.Title, opt => opt.MapFrom(src => src.Name))
                .ReverseMap();

            CreateMap<List<CourseDetailDto>, CourseDetailListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<JobPostingSkill, UpDiddyLib.Dto.SkillDto>()
            .ForMember(c => c.SkillGuid, opt => opt.MapFrom(src => src.Skill.SkillGuid))
            .ForMember(c => c.SkillName, opt => opt.MapFrom(src => src.Skill.SkillName))
            .ForAllOtherMembers(opts => opts.Ignore());

            CreateMap<UpDiddyLib.Dto.SkillDto, JobPostingSkill>()
               .ForPath(c => c.Skill.SkillGuid, opt => opt.MapFrom(src => src.SkillGuid))
               .ForPath(c => c.Skill.SkillName, opt => opt.MapFrom(src => src.SkillName))
               .ForAllOtherMembers(opts => opts.Ignore());


            CreateMap<JobPosting, UpDiddyLib.Dto.JobPostingDto>()
                .ForMember(x => x.MetaDescription, opt => opt.Ignore())
                .ForMember(x => x.MetaTitle, opt => opt.Ignore())
                .ReverseMap();




            CreateMap<JobPosting, JobDetailDto>()
                .ForMember(x => x.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName))
                .ForMember(x => x.CommuteTime, opt => opt.Ignore())
                .ReverseMap();


            CreateMap<JobApplication, JobApplicationApplicantViewDto>()
              .ForMember(c => c.JobApplicationGuid, opt => opt.MapFrom(src => src.JobApplicationGuid))
              .ForMember(c => c.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(c => c.JobPosting, opt => opt.MapFrom(src => src.JobPosting))
              .ForMember(c => c.JobPostingUrl, opt => opt.MapFrom(src => src.JobPosting.JobPostingGuid))
              .ForMember(c => c.CoverLetter, opt => opt.MapFrom(src => src.CoverLetter))
              .ForAllOtherMembers(opts => opts.Ignore());

            CreateMap<JobApplication, JobApplicationRecruiterViewDto>()
              .ForMember(c => c.JobApplicationGuid, opt => opt.MapFrom(src => src.JobApplicationGuid))
              .ForMember(c => c.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(c => c.Subscriber, opt => opt.MapFrom(src => src.Subscriber))
              .ForMember(c => c.CoverLetter, opt => opt.MapFrom(src => src.CoverLetter))
              .ForAllOtherMembers(opts => opts.Ignore());

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

            CreateMap<JobViewDto, UpDiddyLib.Domain.Models.JobPostingDto>().ReverseMap();
            CreateMap<Skill, UpDiddyLib.Domain.Models.SkillDto>()
                .ForMember(c => c.Name, opt => opt.MapFrom(src => src.SkillName))
                .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.SkillDto>, UpDiddyLib.Domain.Models.SkillListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            // todo: had difficulty mapping JObject to Dictionary<string,string> via automapper for PartnerContact -> ContactDto, revisit later if time allows
            //CreateMap<Contact, ContactDto>().ReverseMap();
            //CreateMap<Contact, EmailContactDto>()
            //.ForMember(c => c.email, opt => opt.MapFrom(src => src.Email))
            //.ForMember(c => c.last_name, opt => opt.MapFrom(src => src.LastName))
            //.ForMember(c => c.first_name, opt => opt.MapFrom(src => src.FirstName))
            //.ForMember(c => c.contact_guid, opt => opt.MapFrom(src => src.ContactGuid))
            //.ReverseMap();

            CreateMap<LeadStatus, LeadStatusDto>()
                .ForMember(ls => ls.Message, opt => opt.MapFrom(src => src.Description))
                .ForMember(ls => ls.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
                .ForMember(ls => ls.LeadStatusId, opt => opt.MapFrom(src => src.LeadStatusId))
                .ForMember(ls => ls.Name, opt => opt.MapFrom(src => src.Name));

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
            CreateMap<Subscriber, UpDiddyLib.Dto.SubscriberDto>()
                .ForMember(s => s.Enrollments, opt => opt.MapFrom(src => src.Enrollments))
                .ForMember(s => s.Skills, opt => opt.MapFrom(src => src.SubscriberSkills.Select(ss => ss.Skill)))
                .ForMember(s => s.WorkHistory, opt => opt.MapFrom(src => src.SubscriberWorkHistory))
                .ForMember(s => s.EducationHistory, opt => opt.MapFrom(src => src.SubscriberEducationHistory))
                .ForMember(s => s.Files, opt => opt.MapFrom(src => src.SubscriberFile))
                .ForMember(s => s.Notifications, opt => opt.MapFrom(src => src.SubscriberNotifications.Select(sn => new
                {
                    sn.Notification.CreateDate,
                    sn.Notification.CreateGuid,
                    sn.Notification.ModifyDate,
                    sn.Notification.ModifyGuid,
                    sn.Notification.IsDeleted,
                    sn.Notification.NotificationGuid,
                    sn.Notification.Title,
                    sn.Notification.Description,
                    sn.Notification.IsTargeted,
                    sn.Notification.ExpirationDate,
                    sn.HasRead
                })))
                .ReverseMap();

            CreateMap<Subscriber, SubscribeProfileBasicDto>()
                .ForMember(x => x.ProvinceCode, opt => opt.MapFrom(src => src.State.Code))
                .ReverseMap();


            CreateMap<Subscriber, SubscriberProfileSocialDto>()
                .ReverseMap();

            CreateMap<SubscriberFile, SubscriberFileDto>()
                .ForMember(s => s.SimpleName, opt => opt.MapFrom(src => src.SimpleName))
                .ForMember(s => s.SubscriberFileGuid, opt => opt.MapFrom(src => src.SubscriberFileGuid))
                .ForMember(s => s.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(s => s.BlobName, opt => opt.MapFrom(src => src.BlobName))
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
                .ForMember(x => x.EducationalDegreeType, opt => opt.MapFrom(src => src.EducationalDegreeType.DegreeType))
                 .ForMember(x => x.EducationalDegreeTypeGuid, opt => opt.MapFrom(src => src.EducationalDegreeType.EducationalDegreeTypeGuid));

            CreateMap<SubscriberNotes, SubscriberNotesDto>()
                .ForMember(s => s.ModifiedDate, opt => opt.MapFrom(src => src.ModifyDate))
                .ForMember(s => s.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<SubscriberNotesDto>, SubscriberNotesListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.SubscriberNotes, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<SubscriberNotification, SubscriberNotificationDto>().ReverseMap();

            CreateMap<UpDiddyApi.Models.Notification, UpDiddyLib.Dto.NotificationDto>()
                .ForMember(dest => dest.HasRead, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UpDiddyApi.Models.Notification, UpDiddyLib.Domain.Models.NotificationDto>()
                .ForMember(dest => dest.HasRead, opt => opt.Ignore())
                .ForMember(dest => dest.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.NotificationDto>, UpDiddyLib.Domain.Models.NotificationListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.Notifications, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<CourseSite, CourseSiteDto>()
                .ForMember(x => x.SyncCount, opt => opt.MapFrom(src => src.CoursePages.Count(cp => cp.CoursePageStatus.Name == "Synced")))
                .ForMember(x => x.CreateCount, opt => opt.MapFrom(src => src.CoursePages.Count(cp => cp.CoursePageStatus.Name == "Create")))
                .ForMember(x => x.UpdateCount, opt => opt.MapFrom(src => src.CoursePages.Count(cp => cp.CoursePageStatus.Name == "Update")))
                .ForMember(x => x.DeleteCount, opt => opt.MapFrom(src => src.CoursePages.Count(cp => cp.CoursePageStatus.Name == "Delete")))
                .ForMember(x => x.ErrorCount, opt => opt.MapFrom(src => src.CoursePages.Count(cp => cp.CoursePageStatus.Name == "Error")))
                .ReverseMap();

            CreateMap<TalentFavorite, TalentFavoriteDto>()
                .ForMember(x => x.SubscriberGuid, opt => opt.MapFrom(src => src.Talent.SubscriberGuid))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Talent.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Talent.LastName))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Talent.Email))
                .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(src => src.Talent.PhoneNumber))
                .ForMember(x => x.ProfileImage, opt => opt.MapFrom(src => src.Talent.ProfileImage))
                .ForMember(x => x.ModifyDate, opt => opt.MapFrom(src => src.Talent.ModifyDate))
                .ForMember(x => x.JoinDate, opt => opt.MapFrom(src => src.Talent.CreateDate))
                .ForMember(x => x.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<TalentFavoriteDto>, TalentFavoriteListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.TalentFavorites, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<CourseEnrollmentDto, BraintreePaymentDto>()
                .ForMember(c => c.PaymentAmount, opt => opt.MapFrom(src => src.PaymentAmount))
                .ForMember(c => c.Nonce, opt => opt.MapFrom(src => src.Nonce))
                .ForMember(c => c.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(c => c.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(c => c.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(c => c.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(c => c.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(c => c.Region, opt => opt.MapFrom(src => src.Region))
                .ForMember(c => c.Locality, opt => opt.MapFrom(src => src.Locality))
                .ForMember(c => c.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
                .ForMember(c => c.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(c => c.MerchantAccountId, opt => opt.MapFrom(src => src.MerchantAccountId))
                .ForMember(c => c.StateGuid, opt => opt.MapFrom(src => src.StateGuid))
                .ForMember(c => c.CountryGuid, opt => opt.MapFrom(src => src.CountryGuid))
                .ForAllOtherMembers(opts => opts.Ignore());


            CreateMap<SubscriberNotification, UpDiddyLib.Dto.NotificationDto>()
            .ForMember(c => c.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
            .ForMember(c => c.CreateGuid, opt => opt.MapFrom(src => src.CreateGuid))
            .ForMember(c => c.Description, opt => opt.MapFrom(src => src.Notification.Description))
            .ForMember(c => c.ExpirationDate, opt => opt.MapFrom(src => src.Notification.ExpirationDate))
            .ForMember(c => c.HasRead, opt => opt.MapFrom(src => src.HasRead))
            .ForMember(c => c.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(c => c.ModifyDate, opt => opt.MapFrom(src => src.ModifyDate))
            .ForMember(c => c.ModifyGuid, opt => opt.MapFrom(src => src.ModifyGuid))
            .ForMember(c => c.NotificationGuid, opt => opt.MapFrom(src => src.Notification.NotificationGuid))
            .ForMember(c => c.Title, opt => opt.MapFrom(src => src.Notification.Title))
            .ForAllOtherMembers(opts => opts.Ignore());

            CreateMap<UpDiddyLib.Dto.JobPostingDto, JobCrudDto>()
                .ForMember(dest => dest.CompanyGuid, opt => opt.MapFrom(src => src.Company.CompanyGuid))
                .ForMember(dest => dest.CompensationTypeGuid, opt => opt.MapFrom(src => src.CompensationType.CompensationTypeGuid))
                .ForMember(dest => dest.EducationLevelGuid, opt => opt.MapFrom(src => src.EducationLevel.EducationLevelGuid))
                .ForMember(dest => dest.EmploymentTypeGuid, opt => opt.MapFrom(src => src.EmploymentType.EmploymentTypeGuid))
                .ForMember(dest => dest.ExperienceLevelGuid, opt => opt.MapFrom(src => src.ExperienceLevel.ExperienceLevelGuid))
                .ForMember(dest => dest.IndustryGuid, opt => opt.MapFrom(src => src.Industry.IndustryGuid))
                .ForMember(dest => dest.JobCategoryGuid, opt => opt.MapFrom(src => src.JobCategory.JobCategoryGuid))
                .ForMember(dest => dest.RecruiterGuid, opt => opt.MapFrom(src => src.Recruiter.RecruiterGuid))
                .ForMember(dest => dest.SecurityClearanceGuid, opt => opt.MapFrom(src => src.SecurityClearance.SecurityClearanceGuid));

            CreateMap<JobCrudDto, UpDiddyLib.Dto.JobPostingDto>()
               .ForMember(x => x.Recruiter, opt => opt.Ignore())
               .ForMember(x => x.Company, opt => opt.Ignore())
               .ForMember(x => x.Industry, opt => opt.Ignore())
               .ForMember(x => x.JobCategory, opt => opt.Ignore())
               .ForMember(x => x.ExperienceLevel, opt => opt.Ignore())
               .ForMember(x => x.EducationLevel, opt => opt.Ignore())
               .ForMember(x => x.CompensationType, opt => opt.Ignore())
               .ForMember(x => x.SecurityClearance, opt => opt.Ignore())
               .ForMember(x => x.EmploymentType, opt => opt.Ignore())
               .ForMember(x => x.CityProvince, opt => opt.Ignore())
               .ForMember(x => x.SimilarJobs, opt => opt.Ignore())
               .ForMember(x => x.EmploymentType, opt => opt.Ignore())
               .ForMember(x => x.RequestId, opt => opt.Ignore())
               .ForMember(x => x.ClientEventId, opt => opt.Ignore())
               .ForMember(x => x.CloudTalentUri, opt => opt.Ignore())
               .ForMember(x => x.CloudTalentIndexStatus, opt => opt.Ignore());

            CreateMap<JobCrudDto, JobPosting>()
               .ForPath(x => x.Recruiter.RecruiterGuid, opt => opt.MapFrom(src => src.RecruiterGuid))
               .ForPath(x => x.Company.CompanyGuid, opt => opt.MapFrom(src => src.CompanyGuid))
               .ForPath(x => x.Industry.IndustryGuid, opt => opt.MapFrom(src => src.IndustryGuid))
               .ForPath(x => x.JobCategory.JobCategoryGuid, opt => opt.MapFrom(src => src.JobCategoryGuid))
               .ForPath(x => x.ExperienceLevel.ExperienceLevelGuid, opt => opt.MapFrom(src => src.ExperienceLevelGuid))
               .ForPath(x => x.EducationLevel.EducationLevelGuid, opt => opt.MapFrom(src => src.EducationLevelGuid))
               .ForPath(x => x.CompensationType.CompensationTypeGuid, opt => opt.MapFrom(src => src.CompensationTypeGuid))
               .ForPath(x => x.SecurityClearance.SecurityClearanceGuid, opt => opt.MapFrom(src => src.SecurityClearanceGuid))
               .ForPath(x => x.EmploymentType.EmploymentTypeGuid, opt => opt.MapFrom(src => src.EmploymentTypeGuid))
               .ForMember(x => x.Recruiter, opt => opt.Ignore())
               .ForMember(x => x.Company, opt => opt.Ignore())
               .ForMember(x => x.Industry, opt => opt.Ignore())
               .ForMember(x => x.JobCategory, opt => opt.Ignore())
               .ForMember(x => x.ExperienceLevel, opt => opt.Ignore())
               .ForMember(x => x.EducationLevel, opt => opt.Ignore())
               .ForMember(x => x.CompensationType, opt => opt.Ignore())
               .ForMember(x => x.SecurityClearance, opt => opt.Ignore())
               .ForMember(x => x.EmploymentType, opt => opt.Ignore())
               .ForMember(x => x.EmploymentType, opt => opt.Ignore())
               .ForMember(x => x.EmploymentType, opt => opt.Ignore())
               .ForMember(x => x.CloudTalentUri, opt => opt.Ignore())
               .ForMember(x => x.CloudTalentIndexStatus, opt => opt.Ignore())
               .ReverseMap();


            CreateMap<List<JobCrudDto>, JobCrudListDto>()
            .AfterMap((src, dest) =>
            {
                if (src != null && src.Count() > 0)
                    dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                else
                    dest.TotalRecords = 0;
            })
            .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => src.ToList()))
            .ReverseMap();

            CreateMap<List<GroupInfoDto>, GroupInfoListDto>()
            .AfterMap((src, dest) =>
            {
                if (src != null && src.Count() > 0)
                    dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                else
                    dest.TotalRecords = 0;
            })
            .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => src.ToList()))
            .ReverseMap();

            CreateMap<List<RecruiterInfoDto>, RecruiterInfoListDto>()
            .AfterMap((src, dest) =>
            {
                if (src != null && src.Count() > 0)
                    dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                else
                    dest.TotalRecords = 0;
            })
            .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => src.ToList()))
            .ReverseMap();

            CreateMap<Recruiter, RecruiterInfoDto>()
               .ForPath(x => x.SubscriberGuid, opt => opt.MapFrom(src => src.Subscriber.SubscriberGuid))
               .ForMember(x => x.TotalRecords, opt => opt.Ignore())
               .ForMember(x => x.IsInAuth0RecruiterGroup, opt => opt.Ignore())
               .ForMember(c => c.RecruiterGuid, opt => opt.MapFrom(src => src.RecruiterGuid))
               .ForMember(c => c.FirstName, opt => opt.MapFrom(src => src.FirstName))
               .ForMember(c => c.LastName, opt => opt.MapFrom(src => src.LastName))
               .ForMember(c => c.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
               .ForMember(c => c.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(c => c.CompanyGuid, opt => opt.MapFrom(src => src.Company.CompanyGuid))
               .ForMember(c => c.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName))
               .ReverseMap();

            CreateMap<SendGridEventDto, SendGridEvent>()
                .ForMember(x => x.SendGridEventGuid, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<JobPostingSkill, UpDiddyLib.Domain.Models.SkillDto>()
                .ForMember(c => c.Name, opt => opt.MapFrom(src => src.Skill.SkillName))
                .ForMember(c => c.SkillGuid, opt => opt.MapFrom(src => src.Skill.SkillGuid))
                .ForMember(x => x.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<UsersDto>, UsersListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                      {
                          dest.TotalEnrollments = src.FirstOrDefault().TotalEnrollments;
                          dest.TotalUsers = src.FirstOrDefault().TotalUsers;
                      }
                      else
                      {
                          dest.TotalEnrollments = 0;
                          dest.TotalUsers = 0;
                      }
                  })
                .ForMember(dest => dest.NewUsers, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();
            
            CreateMap<Subscriber, SubscriberSDOC>()
            .ForMember(c => c.SubscriberGuid, opt => opt.MapFrom(src => src.SubscriberGuid))
            .ForMember(c => c.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(c => c.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(c => c.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(c => c.Email, opt => opt.MapFrom(src => src.Email))
            .ForAllOtherMembers(opt => opt.Ignore());
            
            CreateMap<Recruiter, RecruiterSDOC>()
            .ForMember(c => c.SubscriberGuid, opt => opt.MapFrom(src => src.Subscriber.SubscriberGuid))
            .ForMember(c => c.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(c => c.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(c => c.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(c => c.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(c => c.RecruiterGuid, opt => opt.MapFrom(src => src.RecruiterGuid))
            .ForMember(c => c.CompanyGuid, opt => opt.MapFrom(src => src.Company.CompanyGuid))
            .ForMember(c => c.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName))
            .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<List<RecruiterStatDto>, RecruiterStatListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                      {
                          dest.TotalOpCoSubmittals = src.FirstOrDefault().TotalOpCoSubmittals;
                          dest.TotalCCSubmittals = src.FirstOrDefault().TotalCCSubmittals;
                          dest.TotalOpCoInterviews = src.FirstOrDefault().TotalOpCoInterviews;
                          dest.TotalCCInterviews = src.FirstOrDefault().TotalCCInterviews;
                          dest.TotalOpCoStarts = src.FirstOrDefault().TotalOpCoStarts;
                          dest.TotalCCStarts = src.FirstOrDefault().TotalCCStarts;
                          dest.TotalOpCoSpread = src.FirstOrDefault().TotalOpCoSpread;
                          dest.TotalCCSpread = src.FirstOrDefault().TotalCCSpread;                      }
                      else
                      {
                          dest.TotalOpCoSubmittals = 0;
                          dest.TotalCCSubmittals = 0;
                          dest.TotalOpCoInterviews = 0;
                          dest.TotalCCInterviews = 0;
                          dest.TotalOpCoStarts = 0;
                          dest.TotalCCStarts = 0;
                          dest.TotalOpCoSpread = 0;
                          dest.TotalCCSpread = 0;
                      }
                  })
                .ForMember(dest => dest.RecruiterStats, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();
            
            CreateMap<v_ProfileAzureSearch, G2SDOC>()
              .ForMember(x => x.Location, opt => opt.Ignore())
              .ReverseMap();

            CreateMap<Models.G2.Profile, ProfileDto>()
                .ForMember(p => p.CityGuid, opt => opt.MapFrom(src => src.City.CityGuid))
                .ForMember(p => p.CompanyGuid, opt => opt.MapFrom(src => src.Company.CompanyGuid))
                .ForMember(p => p.EmploymentTypeGuid, opt => opt.MapFrom(src => src.EmploymentType.EmploymentTypeGuid))
                .ForMember(p => p.ExperienceLevelGuid, opt => opt.MapFrom(src => src.ExperienceLevel.ExperienceLevelGuid))
                .ForMember(p => p.PostalGuid, opt => opt.MapFrom(src => src.Postal.PostalGuid))
                .ForMember(p => p.StateGuid, opt => opt.MapFrom(src => src.State.StateGuid))
                .ForMember(p => p.SubscriberGuid, opt => opt.MapFrom(src => src.Subscriber.SubscriberGuid))
                .ReverseMap();

            CreateMap<List<WishlistDto>, WishlistListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
                .ForMember(dest => dest.Wishlists, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<List<ProfileWishlistDto>, ProfileWishlistListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
                .ForMember(dest => dest.WishlistProfiles, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<Models.G2.Wishlist, WishlistDto>()
                .ForMember(w => w.RecruiterGuid, opt => opt.MapFrom(src => src.Recruiter.RecruiterGuid))
                .ForMember(w => w.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Models.G2.ProfileComment, CommentDto>()
                .ForMember(c => c.CommentGuid, opt => opt.MapFrom(src => src.ProfileCommentGuid))
                .ForMember(c => c.RecruiterGuid, opt => opt.MapFrom(src => src.Recruiter.RecruiterGuid))
                .ForMember(c => c.ProfileGuid, opt => opt.MapFrom(src => src.Profile.ProfileGuid))
                .ForMember(c => c.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<CommentDto>, CommentListDto>()
                .AfterMap((src, dest) =>
                {
                    if (src != null && src.Count() > 0)
                        dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                    else
                        dest.TotalRecords = 0;
                })
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<Tag, UpDiddyLib.Domain.Models.TagDto>()
                .ForMember(t => t.TotalRecords, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<List<UpDiddyLib.Domain.Models.TagDto>, TagListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();

            CreateMap<List<ProfileTagDto>, ProfileTagListDto>()
                  .AfterMap((src, dest) =>
                  {
                      if (src != null && src.Count() > 0)
                          dest.TotalRecords = src.FirstOrDefault().TotalRecords;
                      else
                          dest.TotalRecords = 0;
                  })
                .ForMember(dest => dest.ProfileTags, opt => opt.MapFrom(src => src.ToList()))
                .ReverseMap();
        }
    }
}