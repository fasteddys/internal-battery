using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore; 
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Factory;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Services;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Threading;


namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ProfileMappingHelper
    {

        #region CC subscriber -> cloud talent profile  mapping helpers
        static public GoogleCloudProfile CreateGoogleProfile(UpDiddyDbContext db, Subscriber subscriber, IList<SubscriberSkill> skills)
        {
 
            GoogleCloudProfile gcp = new GoogleCloudProfile()
            {
                name = subscriber.CloudTalentUri,
                externalId = subscriber.SubscriberGuid.ToString(),
                createTime = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriber.CreateDate),
                    Nanos = 0

                },
                updateTime = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriber.ModifyDate.Value),
                    Nanos = 0
                },
                personNames = MapPersonName(subscriber),
                addresses = MapAddress(subscriber),
                skills = MapSkill(skills),
                employmentRecords = MapWorkHistory(subscriber),
                educationRecords = MapEducationHistory(subscriber),
                emailAddresses = MapEmailAddress(subscriber),
                phoneNumbers = MapPhoneNumber(subscriber)
            };

            // mark the profile with the name of the partner who is resposnible for the 
            // subscriber joining careercircle 
            var partnerInfo = db.SubscriberSignUpPartnerReferences
                .Where(p => p.SubscriberId == subscriber.SubscriberId)
                .FirstOrDefault();

            if ( partnerInfo != null )
            {
                Partner partner = db.Partner
                    .Where(p => p.PartnerId == partnerInfo.PartnerId)
                    .FirstOrDefault();
                if (partner != null)
                {
                    gcp.customAttributes = new Dictionary<string, CustomAttribute>();
                    gcp.customAttributes["SourcePartner"] = new CustomAttribute
                    {
                        stringValues = new[] { partner.Name },
                        filterable = true
                    };
                }
            }                
            return gcp;
        }

        #endregion

        #region cloud talent profile -> profile view  mapping helpers 

        public static ProfileSearchResultDto MapSearchResults(ILogger syslog, IMapper mapper, IConfiguration configuration, SearchProfileResponse searchProfileResponse, ProfileQueryDto profileQuery)
        {

            ProfileSearchResultDto rVal = new ProfileSearchResultDto();

     
            // handle case of no jobs found 
            if (searchProfileResponse == null || searchProfileResponse.summarizedProfiles == null || searchProfileResponse.summarizedProfiles.Count <= 0)
            {
                rVal.JobCount = 0;
                rVal.TotalHits = 0;
                rVal.NumPages = 0;
                return rVal;
            }

            rVal.JobCount = searchProfileResponse.summarizedProfiles.Count;
            rVal.TotalHits = searchProfileResponse.estimatedTotalSize;
            rVal.RequestId = searchProfileResponse.responseMetadata?.requestId;
            rVal.PageSize = profileQuery.PageSize;
            rVal.NumPages = rVal.PageSize != 0 ? (int)Math.Ceiling((double)rVal.TotalHits / rVal.PageSize) : 0;

            rVal.Profiles = new List<ProfileViewDto>();

            foreach (SummarizedProfile p in searchProfileResponse.summarizedProfiles)
            {
                try
                {
                    // Automapper is too slow so do the mapping the old fashion way                    
                    ProfileViewDto pv = CreateProfileView(p);
                    // Map commute properties 
           Startup Here with exception 
 
                    rVal.Profiles.Add(pv);
                }
                catch (Exception e)
                {
                    syslog.LogError(e, "JobPostingFactory.MapSearchResults Error mapping job", e, p);
                }
            }
 

            return rVal;
        }



        public static ProfileViewDto CreateProfileView(SummarizedProfile summarizedProfile)
        {
            ProfileViewDto rVal = new ProfileViewDto()
            {
                SubscriberGuid = Guid.Parse(summarizedProfile.summary.externalId),
                Email = summarizedProfile.summary.emailAddresses?.FirstOrDefault().emailAddress,
                PhoneNumber = summarizedProfile.summary.phoneNumbers?.FirstOrDefault().number,
                FirstName = summarizedProfile.summary.personNames?.FirstOrDefault().structuredName.givenName,
                LastName = summarizedProfile.summary.personNames?.FirstOrDefault().structuredName.familyName,
                Address = summarizedProfile.summary.addresses?.FirstOrDefault().structuredAddress?.addressLines.FirstOrDefault(),
                PostalCode = summarizedProfile.summary.addresses?.FirstOrDefault().structuredAddress?.postalCode,
                City = summarizedProfile.summary.addresses?.FirstOrDefault().structuredAddress?.locality,
                StateCode = summarizedProfile.summary.addresses?.FirstOrDefault().structuredAddress?.administrativeArea
            };
            // add skills 
            rVal.Skills = new List<SkillDto>();
            foreach ( Skill s in summarizedProfile.summary.skills )
            {
                SkillDto skillDto = new SkillDto()
                {
                    SkillName = s.displayName
                };
                rVal.Skills.Add(skillDto);
            }
            // add work history 
            rVal.WorkHistory = new List<SubscriberWorkHistoryDto>();
            foreach ( EmploymentRecord er in summarizedProfile.summary.employmentRecords )
            {
                SubscriberWorkHistoryDto subscriberWorkHistoryDto = new SubscriberWorkHistoryDto()
                {
                    Company = er.employerName,
                    StartDate = er.startDate.ToDate(),
                    EndDate = er.endDate.ToDate(),
                    JobDescription = er.jobDescription,
                    Title = er.jobTitle                    
                };
                rVal.WorkHistory.Add(subscriberWorkHistoryDto);
            }
            // add education history 
            rVal.EducationHistory = new List<SubscriberEducationHistoryDto>();
            foreach (EducationRecord er in summarizedProfile.summary.educationRecords)
            {
                SubscriberEducationHistoryDto subscriberEducationHistoryDto = new SubscriberEducationHistoryDto()
                {
                    EducationalInstitution = er.schoolName,
                    StartDate = er.startDate.ToDate(),
                    EndDate = er.endDate.ToDate(),
                     EducationalDegree = er.structuredDegree.degreeName,
                    EducationalDegreeType = er.structuredDegree?.fieldsOfStudy.FirstOrDefault()
                 };
                rVal.EducationHistory.Add(subscriberEducationHistoryDto);
            }

            return rVal;
        }


        #endregion

        #region helper functions 





        static private List<PersonName> MapPersonName(Subscriber subscriber)
        {
            List <PersonName> rVal = new List<PersonName>();
            PersonName personName = new PersonName(subscriber);
            rVal.Add(personName);
            return rVal;
        }

        static private List<Address> MapAddress(Subscriber subscriber)
        {
            List<Address> rVal = new List<Address>();
            Address address = new Address(subscriber);
            rVal.Add(address);
            return rVal;
        }

        static private List<Skill> MapSkill(IList<SubscriberSkill> skills)
        {
            List<Skill> rVal = new List<Skill>();
            foreach (SubscriberSkill ss in skills)
            {
                Skill skill = new Skill(ss);
                rVal.Add(skill);
            }            
            return rVal;
        }

        static private List<EmploymentRecord> MapWorkHistory(Subscriber subscriber)
        {
            if (subscriber.SubscriberWorkHistory == null || subscriber.SubscriberWorkHistory.Count == 0)
                return null;

            List<EmploymentRecord> rVal = new List<EmploymentRecord>();
            foreach (SubscriberWorkHistory swh in subscriber.SubscriberWorkHistory)
            {
                EmploymentRecord employmentRecord = new EmploymentRecord(swh);
                rVal.Add(employmentRecord);
            }
            return rVal;
        }

        static private List<EducationRecord> MapEducationHistory(Subscriber subscriber)
        {
            if (subscriber.SubscriberEducationHistory == null || subscriber.SubscriberEducationHistory.Count == 0)
                return null;

            List<EducationRecord> rVal = new List<EducationRecord>();
            foreach (SubscriberEducationHistory seh in subscriber.SubscriberEducationHistory)
            {
                EducationRecord educationRecord = new EducationRecord(seh);
                rVal.Add(educationRecord);
            }
            return rVal;
        }


        static private List<EmailAddress> MapEmailAddress(Subscriber subscriber)
        {
            List<EmailAddress> rVal = new List<EmailAddress>();
            EmailAddress email = new EmailAddress(subscriber);
            rVal.Add(email);
            return rVal;
        }

        static private List<PhoneNumber> MapPhoneNumber(Subscriber subscriber)
        {
            List<PhoneNumber> rVal = new List<PhoneNumber>();
            PhoneNumber phoneNumber = new PhoneNumber(subscriber);
            rVal.Add(phoneNumber);
            return rVal;
        }



        #endregion



    }





}
