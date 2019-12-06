﻿using System;
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
            CreateMap<Vendor, VendorDto>().ReverseMap();
            CreateMap<Enrollment, EnrollmentDto>().ReverseMap();
            CreateMap<WozCourseEnrollment, WozCourseEnrollmentDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<EnrollmentLog, EnrollmentLogDto>().ReverseMap();
            CreateMap<CourseVariantType, CourseVariantTypeDto>().ReverseMap();
            CreateMap<Skill, UpDiddyLib.Dto.SkillDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<EducationalInstitution, EducationalInstitutionDto>().ReverseMap();
            CreateMap<EducationalDegree, EducationalDegreeDto>().ReverseMap();
            CreateMap<EducationalDegreeType, EducationalDegreeTypeDto>().ReverseMap();
            CreateMap<CompensationType, UpDiddyLib.Dto.CompensationTypeDto>().ReverseMap();
            CreateMap<Campaign, CampaignDto>().ReverseMap();
            CreateMap<CampaignCourseVariant, CampaignCourseVariantDto>().ReverseMap();
            CreateMap<RebateType, RebateTypeDto>().ReverseMap();
            CreateMap<Refund, RefundDto>().ReverseMap();

            CreateMap<CampaignStatistic, CampaignStatisticDto>().ReverseMap();
            CreateMap<CampaignDetail, CampaignDetailDto>().ReverseMap();
            CreateMap<v_SubscriberSources, SubscriberSourceStatisticDto>().ReverseMap();
            CreateMap<SecurityClearance, UpDiddyLib.Dto.SecurityClearanceDto>().ReverseMap();
            CreateMap<EmploymentType, UpDiddyLib.Dto.EmploymentTypeDto>().ReverseMap();
            CreateMap<Industry, IndustryDto>().ReverseMap();

            CreateMap<JobPostingSkill, JobPostingSkillDto>().ReverseMap();
            CreateMap<ExperienceLevel, UpDiddyLib.Dto.ExperienceLevelDto>().ReverseMap();
            CreateMap<EducationLevel, EducationLevelDto>().ReverseMap();
            CreateMap<JobCategory, JobCategoryDto>().ReverseMap();
            CreateMap<JobApplication, JobApplicationDto>().ReverseMap();
            CreateMap<RecruiterCompany, RecruiterCompanyDto>().ReverseMap();
            CreateMap<JobPostingFavorite, JobPostingFavoriteDto>().ReverseMap();
            CreateMap<Recruiter, RecruiterDto>().ReverseMap();
            CreateMap<JobSite, JobSiteDto>().ReverseMap();
            CreateMap<JobSiteScrapeStatistic, JobSiteScrapeStatisticDto>().ReverseMap();
            CreateMap<ResumeParse, ResumeParseDto>().ReverseMap();
            CreateMap<ResumeParseResult, ResumeParseResultDto>().ReverseMap();
            CreateMap<Subscriber, FailedSubscriberDto>().ReverseMap();
            CreateMap<CourseLevel, CourseLevelDto>().ReverseMap();
            CreateMap<RedemptionStatus, RedemptionStatusDto>().ReverseMap();
            CreateMap<ServiceOffering, ServiceOfferingDto>().ReverseMap();
            CreateMap<ServiceOfferingItem, ServiceOfferingItemDto>().ReverseMap();
            CreateMap<ServiceOfferingOrder, ServiceOfferingOrderDto>().ReverseMap();
            CreateMap<ServiceOfferingPromoCodeRedemption, ServiceOfferingPromoCodeRedemptionDto>().ReverseMap();
            CreateMap<FileDownloadTracker, FileDownloadTrackerDto>().ReverseMap();
            CreateMap<Traitify, TraitifyDto>().ReverseMap();
            CreateMap<Course, CourseDetailDto>()
                .ForMember(c => c.VendorLogoUrl, opt => opt.MapFrom(src => src.Vendor.LogoUrl))
                .ForMember(c => c.ThumbnailUrl, opt => opt.MapFrom(src => src.TabletImage))
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
            CreateMap<Skill,UpDiddyLib.Domain.Models.SkillDto>()
            .ForMember(c => c.Name, opt => opt.MapFrom(src => src.SkillName)).ReverseMap();



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
                .ForMember(x => x.EducationalDegreeType, opt => opt.MapFrom(src => src.EducationalDegreeType.DegreeType));

            CreateMap<SubscriberNotes, SubscriberNotesDto>()
                .ForMember(s => s.ModifiedDate, opt => opt.MapFrom(src => src.ModifyDate))
                .ReverseMap();

            CreateMap<SubscriberNotification, SubscriberNotificationDto>().ReverseMap();
            CreateMap<Notification, NotificationDto>()
                .ForMember(n => n.HasRead, opt => opt.Ignore());

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
                .ReverseMap();


        }


    }
}