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
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ProfileMappingHelper
    {

        #region CC subscriber -> cloud talent profile  mapping helpers
        static public GoogleCloudProfile CreateGoogleProfile(IRepositoryWrapper repositoryWrapper, int maxSkillLen, Subscriber subscriber, IList<SubscriberSkill> skills, ISubscriberService subscriberService)
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
                skills = MapSkill(maxSkillLen, skills),
                employmentRecords = MapWorkHistory(subscriber),
                educationRecords = MapEducationHistory(subscriber),
                emailAddresses = MapEmailAddress(subscriber),
                phoneNumbers = MapPhoneNumber(subscriber)
            };

 
            string partnerSource = Constants.NotSpecifedOption;
            //  search for the first partner attributed source 
            IList<SubscriberSourceDto> subscriberSourceDtos = subscriberService.GetSubscriberSources(subscriber.SubscriberId).Result;
            foreach ( SubscriberSourceDto s in subscriberSourceDtos)
            {
                if ( s.GroupRank == 1 && s.PartnerRank == 1 )
                {
                    partnerSource = s.PartnerName;
                    break;
                }
            }
         
            List<string> partnerList = new List<string>();
            partnerList.Add(partnerSource);
           
            gcp.customAttributes = new Dictionary<string, CustomAttribute>();
            // index source partner as custom attribute 
            gcp.customAttributes["SourcePartner"] = new CustomAttribute
            {
                stringValues = partnerList.ToArray(), 
                filterable = true
                 
            };
            // index email address as custom attribute
            gcp.customAttributes["EmailAddress"] = new CustomAttribute
            {
                stringValues = new[] { subscriber.Email.Trim() },
                filterable = true
            };

            if ( ! string.IsNullOrEmpty(subscriber.FirstName ))
            {
                gcp.customAttributes["FirstName"] = new CustomAttribute
                {
                    stringValues = new[] { subscriber.FirstName.Trim() },
                    filterable = true
                };
            }

            gcp.customAttributes["CreateDate"] = new CustomAttribute
            {
                    stringValues = new[] { subscriber.CreateDate.ToShortDateString() },
                    filterable = true
            };
          
            
            if ( subscriber.ModifyDate != null)
            {
                gcp.customAttributes["ModifyDate"] = new CustomAttribute
                {
                    stringValues = new[] { subscriber.ModifyDate.Value.ToShortDateString() },
                    filterable = true
                };
            }
            



            if (!string.IsNullOrEmpty(subscriber.LastName))
            {
                gcp.customAttributes["LastName"] = new CustomAttribute
                {
                    stringValues = new[] { subscriber.LastName.Trim() },
                    filterable = true
                };
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
                rVal.ProfileCount = 0;
                rVal.TotalHits = 0;
                rVal.NumPages = 0;
                return rVal;
            }

            rVal.ProfileCount = searchProfileResponse.summarizedProfiles.Count;
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
            ProfileViewDto rVal = new ProfileViewDto();


            rVal.CloudTalentUri = summarizedProfile.profiles[0]?.name;


            Guid SubscriberGuid = Guid.Empty;
            if (Guid.TryParse(summarizedProfile.summary.externalId, out SubscriberGuid))
                 rVal.SubscriberGuid = SubscriberGuid;


            // not using linq to make things as speedy as possible 
            // email address 
            if (summarizedProfile.summary.emailAddresses != null && summarizedProfile.summary.emailAddresses.Count > 0)
                rVal.Email = summarizedProfile.summary.emailAddresses[0].emailAddress;

            // phone number 
            if (summarizedProfile.summary.phoneNumbers != null && summarizedProfile.summary.phoneNumbers.Count > 0)
                rVal.PhoneNumber = summarizedProfile.summary.phoneNumbers[0].number;

            // first and last names 
            if (summarizedProfile.summary.personNames != null && summarizedProfile.summary.personNames.Count > 0 )
            {
                if (summarizedProfile.summary.personNames[0].structuredName != null )
                {
                    rVal.FirstName = summarizedProfile.summary.personNames[0].structuredName.givenName;
                    rVal.LastName = summarizedProfile.summary.personNames[0].structuredName.familyName;
                }                
            }

            if (summarizedProfile.summary.addresses != null && summarizedProfile.summary.addresses.Count > 0)
            {
                if (summarizedProfile.summary.addresses[0].structuredAddress != null)
                {
                    rVal.PostalCode = summarizedProfile.summary.addresses[0].structuredAddress.postalCode;
                    rVal.City = summarizedProfile.summary.addresses[0].structuredAddress.locality;
                    rVal.StateCode = summarizedProfile.summary.addresses[0].structuredAddress.administrativeArea;

                    if (summarizedProfile.summary.addresses[0].structuredAddress.addressLines != null && summarizedProfile.summary.addresses[0].structuredAddress.addressLines.Count > 0 )
                    {
                        rVal.Address = summarizedProfile.summary.addresses[0].structuredAddress.addressLines[0];
                    }
                }
            }

            
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
                    StartDate = er.startDate?.ToDate(),
                    EndDate = er.endDate?.ToDate(),
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
                    StartDate = er.startDate?.ToDate(),
                    EndDate = er.endDate?.ToDate(),
                    EducationalDegree = er.structuredDegree?.degreeName,
                    EducationalDegreeType = er.structuredDegree?.fieldsOfStudy.FirstOrDefault()
                 };
                rVal.EducationHistory.Add(subscriberEducationHistoryDto);
            }
 
            // map source partner 
            rVal.SourcePartner = new List<string>();
     
            if (summarizedProfile.summary.customAttributes != null && summarizedProfile.summary.customAttributes.ContainsKey("sourcePartner"))
                foreach ( string s in summarizedProfile.summary.customAttributes["sourcePartner"].stringValues)
                    rVal.SourcePartner.Add(s);
            else
                rVal.SourcePartner.Add(Constants.NotSpecifedOption);

            // map create date
            DateTime createDate  = DateTime.MinValue;
            if (summarizedProfile.summary.customAttributes != null && summarizedProfile.summary.customAttributes.ContainsKey("createDate"))
                 DateTime.TryParse(summarizedProfile.summary.customAttributes["createDate"].stringValues[0], out createDate);
             rVal.CreateDate = createDate;

            // map modify date
            DateTime modifyDate = DateTime.MinValue;
            if (summarizedProfile.summary.customAttributes != null && summarizedProfile.summary.customAttributes.ContainsKey("modifyDate"))
                DateTime.TryParse(summarizedProfile.summary.customAttributes["modifyDate"].stringValues[0], out modifyDate);
            rVal.ModifyDate = modifyDate;


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

        static private List<Skill> MapSkill(int maxSkillLen, IList<SubscriberSkill> skills)
        {
        
            List<Skill> rVal = new List<Skill>();
            foreach (SubscriberSkill ss in skills)
            {
                if ( ss.Skill != null && ss.Skill.SkillName.Length <= maxSkillLen )
                {
                    Skill skill = new Skill(ss);
                    rVal.Add(skill);
                }                
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
