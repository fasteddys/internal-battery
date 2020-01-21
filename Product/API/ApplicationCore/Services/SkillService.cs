
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
namespace UpDiddyApi.ApplicationCore.Services
{
    public class SkillService : ISkillService
    {
        private IConfiguration _configuration { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repositoryWrapper { get; set; }
        private readonly IMapper _mapper;

        private readonly ISubscriberService _subscriberService;

        public SkillService(
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            ISubscriberService subscriberService,
            IMapper mapper)
        {
            _configuration = configuration;
            _repositoryWrapper = repository;
            _logger = logger;
            _mapper = mapper;
            _subscriberService = subscriberService;
        }

        public async Task<SkillListDto> GetSkills(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var skills = await _repositoryWrapper.StoredProcedureRepository.GetSkills(limit, offset, sort, order);
            if (skills == null)
                throw new NotFoundException("Skills not found");
            return _mapper.Map<SkillListDto>(skills);
        }

        public async Task<SkillDto> GetSkill(Guid skillGuid)
        {
            var skill = await _repositoryWrapper.SkillRepository.GetByGuid(skillGuid);
            if (skill == null)
                throw new NotFoundException("Skill not found");
            return _mapper.Map<SkillDto>(skill);
        }

        public async Task CreateSkill(SkillDto skillDto)
        {
            if (skillDto == null)
                throw new NullReferenceException("SkillDto cannot be null");
            Skill skill = _mapper.Map<Skill>(skillDto);
            skill.CreateDate = DateTime.UtcNow;
            skill.SkillGuid = Guid.NewGuid();
            await _repositoryWrapper.SkillRepository.Create(skill);
            await _repositoryWrapper.SaveAsync();
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
            }

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


    }
}
