using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.B2B;
using UpDiddyLib.Domain.Models.B2B;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class HiringManagerRepository : UpDiddyRepositoryBase<HiringManager>, IHiringManagerRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public HiringManagerRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HiringManager> GetHiringManagerBySubscriberId(int SubscriberId)
        {
            var hiringManager = await _dbContext.HiringManager.Where(hm => hm.SubscriberId == SubscriberId && hm.IsDeleted == 0)
                .Include(hm => hm.Company).ThenInclude(c => c.Industry)
                .Include(hm => hm.Subscriber).ThenInclude(s => s.State)
                .FirstOrDefaultAsync();

            return hiringManager;
        }

        public async Task AddHiringManager(int subscriberId)
        {
            _dbContext.HiringManager.Add(new HiringManager
            {
                SubscriberId = subscriberId,
                HiringManagerGuid = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow,
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<CandidateDetailDto> GetCandidate360Detail(Guid profileGuid)
        {
            var profile = _dbContext.Profile
                .Include(p => p.Subscriber).ThenInclude(s => s.State)
                .Where(p => p.IsDeleted == 0 && p.ProfileGuid == profileGuid)
                .FirstOrDefault();

            if (profile == null || profile.Subscriber == null)
                throw new NotFoundException($"Profile not found: {profileGuid}");

            var skills = _dbContext.SubscriberSkill
                .Include(ss => ss.Skill)
                .Where(ss => ss.IsDeleted == 0 && ss.SubscriberId == profile.SubscriberId)
                .Select(ss => ss.Skill.SkillName)
                .ToList();

            string location = null;
            if (profile.Subscriber.City != null && profile.Subscriber.State != null)
            {
                location = $"{profile.Subscriber.City}, {profile.Subscriber.State.Code}";
            }
            else if (profile.Subscriber.City != null)
            {
                location = profile.Subscriber.City;
            }
            else if (profile.Subscriber.State != null)
            {
                location = profile.Subscriber.State.Name;
            }

            decimal? desiredAnnualSalary = null;
            if (profile.Subscriber.DesiredSalary.HasValue)
            {
                desiredAnnualSalary = profile.Subscriber.DesiredSalary.Value;
            }
            else if (profile.Subscriber.DesiredRate.HasValue)
            {
                desiredAnnualSalary = profile.Subscriber.DesiredRate.Value * 40 * 52;
            }

            decimal? estimatedHiringFee = null;
            if (desiredAnnualSalary.HasValue)
            {
                estimatedHiringFee = desiredAnnualSalary * 0.2M;
            }

            var employmentPreferences = new List<string>();
            var subscriberEmploymentTypes = _dbContext.SubscriberEmploymentTypes
                .Include(set => set.EmploymentType)
                .Where(set => set.IsDeleted == 0 && set.SubscriberId == profile.SubscriberId)
                .Select(set => set.EmploymentType.Name)
                .ToList();
            if (subscriberEmploymentTypes != null)
                employmentPreferences.AddRange(subscriberEmploymentTypes);
            if (profile.Subscriber.IsWillingToTravel.HasValue)
            {
                if (profile.Subscriber.IsWillingToTravel.Value)
                    employmentPreferences.Add("Will Travel");
                else
                    employmentPreferences.Add("On-site");
            }
            if (profile.Subscriber.IsFlexibleWorkScheduleRequired.HasValue && profile.Subscriber.IsFlexibleWorkScheduleRequired.Value)
                employmentPreferences.Add("Flex");

            var technicalAndProfessionalTraining = _dbContext.SubscriberTraining
            .Where(st => st.IsDeleted == 0 && st.Subscriber.IsDeleted == 0 && st.SubscriberId == profile.SubscriberId)
            .Include(st => st.Subscriber)
            .Include(st => st.TrainingType)
            .Select(st => new HiringManagerTechnicalAndProfessionalTrainingDto() { Concentration = st.TrainingName, Institution = st.TrainingInstitution })
            .ToList();

            var formalEducation = _dbContext.SubscriberEducationHistory
                .Where(seh => seh.SubscriberId == profile.SubscriberId && seh.IsDeleted == 0)
                .Include(seh => seh.EducationalDegree)
                .Include(seh => seh.EducationalInstitution)
                .Include(seh => seh.EducationalDegreeType)
                .Include(seh => seh.EducationalDegreeType.EducationalDegreeTypeCategory)
                .Select(seh => new HiringManagerFormalEducationDto()
                {
                    Concentration = seh.EducationalDegree != null ? seh.EducationalDegree.Degree : null,
                    DegreeType = seh.EducationalDegreeType != null ? seh.EducationalDegreeType.EducationalDegreeTypeCategory.Name + " - " + seh.EducationalDegreeType.DegreeType : null,
                    Institution = seh.EducationalInstitution != null ? seh.EducationalInstitution.Name : null
                })
                .ToList();

            var languages = _dbContext.SubscriberLanguageProficiencies
                .Include(slp => slp.Language)
                .Include(slp => slp.ProficiencyLevel)
                .Where(slp => slp.IsDeleted == 0 && slp.SubscriberId == profile.SubscriberId)
                .Select(slp => new HiringManagerLanguageDto() { Name = slp.Language.LanguageName, Proficiency = slp.ProficiencyLevel.ProficiencyLevelName })
                .ToList();

            HiringManagerTraitifyDto traitify = null;
            if (profile.Subscriber.IsTraitifyAssessmentsVisibleToHiringManagers.HasValue && profile.Subscriber.IsTraitifyAssessmentsVisibleToHiringManagers.Value)
            {
                var rawTraitify = _dbContext.Traitify
                    .Where(t => t.SubscriberId == profile.SubscriberId && t.IsDeleted == 0 && !string.IsNullOrWhiteSpace(t.ResultData))
                    .OrderByDescending(t => t.CreateDate)
                    .FirstOrDefault();
                if (rawTraitify != null)
                {
                    var personalityBlendName = JObject.Parse(rawTraitify.ResultData).SelectToken("personality_blend.name");
                    var personalityBlendImage1 = JObject.Parse(rawTraitify.ResultData).SelectToken("personality_blend.personality_type_1.badge.image_small");
                    var personalityBlendImage2 = JObject.Parse(rawTraitify.ResultData).SelectToken("personality_blend.personality_type_2.badge.image_small");
                    if (personalityBlendName != null && personalityBlendImage1 != null && personalityBlendImage2 != null)
                    {
                        traitify = new HiringManagerTraitifyDto()
                        {
                            PersonalityBlendName = personalityBlendName.Value<string>(),
                            Personality1ImageUrl = personalityBlendImage1.Value<string>(),
                            Personality2ImageUrl = personalityBlendImage2.Value<string>()
                        };
                    }
                }
            }

            var workHistories = _dbContext.SubscriberWorkHistory
                .Include(swh => swh.Company)
                .Where(swh => swh.IsDeleted == 0 && swh.SubscriberId == profile.SubscriberId)
                .OrderByDescending(swh => swh.EndDate).ThenByDescending(swh => swh.CreateDate)
                .Select(swh => new HiringManagerWorkHistoryDto()
                {
                    Company = swh.Company != null ? swh.Company.CompanyName : null,
                    Description = swh.JobDescription,
                    Position = swh.Title,
                    StartDate = swh.StartDate.HasValue ? swh.StartDate.Value.ToString("MM/yy") : null,
                    EndDate = swh.EndDate.HasValue ? swh.EndDate.Value.ToString("MM/yy") : null
                })
                .ToList();

            CandidateDetailDto candidateDetailDto = new CandidateDetailDto()
            {
                ProfileGuid = profile.ProfileGuid,
                FirstName = profile.Subscriber.FirstName,
                JobTitle = profile.Subscriber.Title,
                Location = location,
                Skills = skills,
                DesiredAnnualSalary = desiredAnnualSalary,
                EstimatedHiringFee = estimatedHiringFee,
                EmploymentPreferences = employmentPreferences,
                FormalEducation = formalEducation,
                Languages = languages,
                TechnicalAndProfessionalTraining = technicalAndProfessionalTraining,
                Traitify = traitify,
                VolunteerOrPassionProjects = profile.Subscriber.PassionProjectsDescription,
                WorkHistories = workHistories
            };

            return candidateDetailDto;
        }

        public async Task UpdateHiringManager(int subscriberId, HiringManagerDto hiringManagerDto)
        {
            var hiringManager = _dbContext.HiringManager
                .Where(hm => hm.SubscriberId == subscriberId)
                .Include(hm => hm.Subscriber)
                .Include(hm => hm.Subscriber.State)
                .Include(hm => hm.Company)
                .Include(hm => hm.Company.Industry)
                .FirstOrDefault();

            //add/update Company details

            var industry = _dbContext.Industry.FirstOrDefault(i => hiringManagerDto.CompanyIndustryGuid.HasValue && i.IndustryGuid == hiringManagerDto.CompanyIndustryGuid.Value);
            var state = _dbContext.State.FirstOrDefault(s => s.StateGuid == hiringManagerDto.StateGuid);
            if (hiringManager.Company == null)
            {
                hiringManager.Company = new Company
                {
                    CompanyGuid = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.NewGuid(),
                    WebsiteUrl = hiringManagerDto.CompanyWebsiteUrl,
                    CompanyName = hiringManagerDto.CompanyName,
                    EmployeeSize = hiringManagerDto.CompanySize,
                    Description = hiringManagerDto.CompanyDescription,
                    Industry = industry
                };
            }
            else
            {
                hiringManager.Company.CompanyName = hiringManagerDto.CompanyName;
                hiringManager.Company.EmployeeSize = hiringManagerDto.CompanySize;
                hiringManager.Company.WebsiteUrl = hiringManagerDto.CompanyWebsiteUrl;
                hiringManager.Company.Description = hiringManagerDto.CompanyDescription;
                hiringManager.Company.ModifyGuid = Guid.NewGuid();
                hiringManager.Company.ModifyDate = DateTime.UtcNow;
                hiringManager.Company.Industry = industry;
            }

            //update subscribers record
            hiringManager.Subscriber.FirstName = hiringManagerDto.FirstName;
            hiringManager.Subscriber.LastName = hiringManagerDto.LastName;
            hiringManager.Subscriber.City = hiringManagerDto.City;
            hiringManager.Subscriber.State = state;
            hiringManager.Subscriber.ModifyDate = DateTime.UtcNow;
            hiringManager.Subscriber.ModifyGuid = Guid.NewGuid();
            hiringManager.Subscriber.Title = hiringManagerDto.Title;
            hiringManager.Subscriber.PhoneNumber = hiringManagerDto.PhoneNumber;


            //update HiringManager record
            hiringManager.HardToFindFillSkillsRoles = hiringManagerDto.HardToFindFillSkillsRoles;
            hiringManager.SkillsRolesWeAreAlwaysHiringFor = hiringManagerDto.SkillsRolesWeAreAlwaysHiringFor;


            await _dbContext.SaveChangesAsync();
        }

    }
}
