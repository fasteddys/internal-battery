using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Factory;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SkillService : ISkillService
    {
        private IConfiguration _configuration { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repositoryWrapper { get; set; }
        private readonly IMapper _mapper;
        private readonly ISubscriberService _subscriberService;
        private readonly ICourseService _courseService;
        private readonly IG2Service _g2Service;
        private readonly IHubSpotService _hubSpotService;
        private readonly IMemoryCacheService _memoryCacheService;

        private const string allSkillsCacheKey = "AllSkills";

        public SkillService(
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            ISubscriberService subscriberService,
            ICourseService courseService,
            IMapper mapper,
            IG2Service g2Service,
            IHubSpotService hubSpotService,
            IMemoryCacheService memoryCacheService)
        {
            _configuration = configuration;
            _repositoryWrapper = repository;
            _logger = logger;
            _mapper = mapper;
            _subscriberService = subscriberService;
            _courseService = courseService;
            _g2Service = g2Service;
            _hubSpotService = hubSpotService;
            _memoryCacheService = memoryCacheService;
        }

        public async Task<SkillListDto> GetSkills(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var skills = await _repositoryWrapper.StoredProcedureRepository.GetSkills(limit, offset, sort, order);
            if (skills == null)
                throw new NotFoundException("Skills not found");
            return _mapper.Map<SkillListDto>(skills);
        }

        public async Task<List<SkillDto>> GetSkillsByKeyword(string keyword)
        {
            List<SkillDto> cachedAllSkills = (List<SkillDto>)_memoryCacheService.GetCacheValue(allSkillsCacheKey);
            if (cachedAllSkills == null)
            {
                cachedAllSkills = await _repositoryWrapper.StoredProcedureRepository.GetSkills(limit: 30000, offset: 0, sort: "modifyDate", order: "descending");

                if (cachedAllSkills == null)
                    throw new NotFoundException("Skills not found");

                _memoryCacheService.SetCacheValue(allSkillsCacheKey, cachedAllSkills, 60 * 24);

            }
            return cachedAllSkills.Where(s => s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<SkillDto> GetSkill(Guid skillGuid)
        {
            var skill = await _repositoryWrapper.SkillRepository.GetByGuid(skillGuid);
            if (skill == null)
                throw new NotFoundException("Skill not found");
            return _mapper.Map<SkillDto>(skill);
        }

        public async Task<Guid> CreateSkill(SkillDto skillDto)
        {
            if (skillDto == null)
                throw new NullReferenceException("SkillDto cannot be null");
            Skill skill = _mapper.Map<Skill>(skillDto);
            skill.CreateDate = DateTime.UtcNow;
            skill.SkillGuid = Guid.NewGuid();
            await _repositoryWrapper.SkillRepository.Create(skill);
            await _repositoryWrapper.SaveAsync();
            //Clear cache
            _memoryCacheService.ClearCacheByKey(allSkillsCacheKey);
            return skill.SkillGuid.Value;
        }

        public async Task UpdateSkill(Guid skillGuid, SkillDto skillDto)
        {
            if (skillDto == null || skillGuid == null || skillGuid == Guid.Empty)
                throw new NullReferenceException("SkillDto cannot be null");
            Skill skill = await _repositoryWrapper.SkillRepository.GetByGuid(skillGuid);
            if (skill == null)
                throw new NotFoundException("Skill not found");
            skill.SkillName = skillDto.Name;
            skill.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
            //Clear cache
            _memoryCacheService.ClearCacheByKey(allSkillsCacheKey);
        }

        public async Task DeleteSkill(Guid skillGuid)
        {
            if (skillGuid == null || skillGuid == Guid.Empty)
                throw new NullReferenceException("SkillDto cannot be null");
            Skill skill = await _repositoryWrapper.SkillRepository.GetByGuid(skillGuid);
            if (skill == null)
                throw new NotFoundException("Skill not found");
            skill.ModifyDate = DateTime.UtcNow;
            skill.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
            //Clear cache
            _memoryCacheService.ClearCacheByKey(allSkillsCacheKey);
        }

        public async Task<List<SkillDto>> GetSkillsByCourseGuid(Guid courseGuid)
        {
            if (courseGuid == null || courseGuid == Guid.Empty)
                throw new NullReferenceException("Course cannot be null");
            var skills = await _repositoryWrapper.SkillRepository.GetByCourseGuid(courseGuid);
            if (skills == null)
                throw new NotFoundException("Skill not found");
            return _mapper.Map<List<Skill>, List<SkillDto>>(skills);
        }

        public async Task<List<SkillDto>> GetSkillsBySubscriberGuid(Guid subscriberGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            var skills = await _repositoryWrapper.SkillRepository.GetBySubscriberGuid(subscriberGuid);
            if (skills == null)
                throw new NotFoundException("Skill not found");
            return _mapper.Map<List<Skill>, List<SkillDto>>(skills);
        }

        public async Task UpdateCourseSkills(Guid courseGuid, List<Guid> skills)
        {
            var course = await _courseService.GetCourse(courseGuid);
            if (course == null)
                throw new NotFoundException("Course not found");

            await _repositoryWrapper.StoredProcedureRepository.UpdateEntitySkills(courseGuid, "Course", skills);
        }

        public async Task UpdateSubscriberSkills(Guid subscriberGuid, List<string> skills)
        {
            var subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            var subscriberSkillsList = await _repositoryWrapper.SubscriberSkillRepository.GetAllSkillsBySubscriberGuid(subscriberGuid);
            foreach (var subscriberSkill in subscriberSkillsList)
            {
                subscriberSkill.IsDeleted = 1;
                if (skills.Contains(subscriberSkill.Skill.SkillName, StringComparer.CurrentCultureIgnoreCase))
                {
                    subscriberSkill.IsDeleted = 0;
                    skills.RemoveAll(n => n.Equals(subscriberSkill.Skill.SkillName, StringComparison.OrdinalIgnoreCase));
                }
            }
            foreach (var skill in skills)
            {
                Skill skillEntity = new Skill();
                var existingSkill = await _repositoryWrapper.SkillRepository.GetByName(skill);
                if (existingSkill == null)
                {
                    skillEntity.SkillGuid = Guid.NewGuid();
                    skillEntity.SkillName = skill;
                    skillEntity.CreateDate = DateTime.UtcNow;
                    skillEntity.ModifyDate = DateTime.UtcNow;
                }
                else
                {
                    skillEntity.SkillId = existingSkill.SkillId;
                }
                SubscriberSkill subscriberSkill = new SubscriberSkill
                {
                    SubscriberId = subscriber.SubscriberId,
                    SubscriberSkillGuid = Guid.NewGuid(),
                    Skill = skillEntity,
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = DateTime.UtcNow
                };
                await _repositoryWrapper.SubscriberSkillRepository.Create(subscriberSkill);
            }
            if (_repositoryWrapper.SubscriberSkillRepository.HasUnsavedChanges())
            {
                await _repositoryWrapper.SaveAsync();
                await _g2Service.G2IndexBySubscriberAsync(subscriberGuid);
                //Call Hubspot to update self curated skills
                await _hubSpotService.AddOrUpdateContactBySubscriberGuid(subscriberGuid);
            }
        }

        public async Task UpdateSubscriberSkillsByGuid(Guid subscriberGuid, List<Guid> skills)
        {
            var subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");

            await _repositoryWrapper.StoredProcedureRepository.UpdateEntitySkills(subscriberGuid, "Subscriber", skills);
            await _g2Service.G2IndexBySubscriberAsync(subscriberGuid);
            // call Hubspot to update self curated skills
            await _hubSpotService.AddOrUpdateContactBySubscriberGuid(subscriberGuid);
        }

        /// <summary>
        /// Lifted this logic from the JobPostingFactory. Given a list of skill names, we find the skill in our system and return
        /// a reference to it. If a skill does not exist, we add it to our system. 
        /// </summary>
        /// <param name="skills"></param>
        /// <returns></returns>
        public async Task<List<SkillDto>> AddOrUpdateSkillsByName(List<string> skillNames)
        {
            var updatedSkills = new List<SkillDto>();
            foreach (var skillName in skillNames)
            {
                var skill = SkillFactory.GetOrAdd(_repositoryWrapper, _memoryCacheService, skillName).Result;
                if (!updatedSkills.Exists(s => s.SkillGuid == skill.SkillGuid))
                    updatedSkills.Add(new SkillDto()
                    {
                        SkillGuid = skill.SkillGuid.Value,
                        Name = skill.SkillName
                    });
            }
            return updatedSkills;
        }

        public async Task UpdateJobPostingSkillsByGuid(Guid jobPostingGuid, List<Guid> skills)
        {
            await _repositoryWrapper.StoredProcedureRepository.UpdateJobPostingSkills(jobPostingGuid, skills);
        }

        public async Task<SkillListDto> GetProfileSkillsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var skills = await _repositoryWrapper.SkillRepository.GetProfileSkillsForRecruiter(profileGuid, subscriberGuid, limit, offset, sort, order);
            return _mapper.Map<SkillListDto>(skills);
        }

        public async Task DeleteSkillsFromProfileForRecruiter(Guid subscriberGuid, List<Guid> profileSkillGuids)
        {
            await _repositoryWrapper.SkillRepository.DeleteSkillsFromProfileForRecruiter(subscriberGuid, profileSkillGuids);
            Guid profileGuid = await _repositoryWrapper.SkillRepository.GetProfileGuidByProfileSkillGuids(profileSkillGuids);
            await _g2Service.G2IndexProfileByGuidAsync(profileGuid);
        }

        public async Task<List<Guid>> AddSkillsToProfileForRecruiter(Guid subscriberGuid, List<Guid> skillGuids, Guid profileGuid)
        {   
            List<Guid> profileSkillGuids = await _repositoryWrapper.SkillRepository.AddSkillsToProfileForRecruiter(subscriberGuid, skillGuids, profileGuid);
            await _g2Service.G2IndexProfileByGuidAsync(profileGuid);
            //Call Hubspot to update G2 skills
            await _hubSpotService.AddOrUpdateContactBySubscriberGuid(subscriberGuid);
            return profileSkillGuids;
        }

        public async Task UpdateProfileSkillsForRecruiter(Guid subscriberGuid, List<Guid> skillGuids, Guid profileGuid)
        {            
            await _repositoryWrapper.SkillRepository.UpdateProfileSkillsForRecruiter(subscriberGuid, skillGuids, profileGuid);
            await _g2Service.G2IndexProfileByGuidAsync(profileGuid);
            //Call Hubspot to update G2 skills
            await _hubSpotService.AddOrUpdateContactBySubscriberGuid(subscriberGuid);
        }
    }
}