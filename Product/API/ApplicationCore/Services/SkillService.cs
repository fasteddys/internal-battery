
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


        public async Task<List<SkillDto>> GetSkillsBySubscriberGuid(Guid subscriberGuid)
        {
            var entity =  await _repositoryWrapper.SkillRepository.GetBySubscriberGuid(subscriberGuid);
            return _mapper.Map<List<Skill>,List<SkillDto>>(entity);
        }

        public async Task CreateSkillForSubscriber(Guid subscriberGuid, List<SkillDto> skills)
        {
            foreach( var skill in skills)
            {
                var subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
                Skill skillEntity = _mapper.Map<Skill>(skill);
                skillEntity.SkillGuid = Guid.NewGuid();
                skillEntity.CreateDate = DateTime.UtcNow;
                skillEntity.ModifyDate = DateTime.UtcNow;
                
                SubscriberSkill subscriberSkill = new SubscriberSkill {
                    SubscriberId = subscriber.SubscriberId,
                    Subscriber = subscriber,
                    SubscriberSkillGuid = Guid.NewGuid(),
                    Skill = skillEntity,
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = DateTime.UtcNow                    
                };

                await _repositoryWrapper.SubscriberSkillRepository.Create(subscriberSkill);
            }
        }

    }
}
