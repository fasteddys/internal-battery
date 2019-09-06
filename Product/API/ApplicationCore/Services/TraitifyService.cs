using System;
using AutoMapper;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TraitifyService : ITraitifyService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ISubscriberService _subscriberService;
        public TraitifyService(IRepositoryWrapper repositoryWrapper, IMapper mapper, ISubscriberService subscriberService)
        {
            _subscriberService = subscriberService;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<TraitifyDto> GetByAssessmentId(string assessmentId)
        {
            var result =  await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
            return _mapper.Map<TraitifyDto>(result);
        }

        public async Task CreateNewAssessment(TraitifyDto dto)
        {
            Subscriber subscriber = null;
            if (dto.SubscriberGuid != null)
            {
                subscriber = await _subscriberService.GetBySubscriberGuid(dto.SubscriberGuid.Value);
            }
            Traitify traitify = new Traitify()
            {
                Subscriber = subscriber != null ? subscriber : null,
                SubscriberId = subscriber != null ? subscriber.SubscriberId : (int?) null,
                TraitifyGuid = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow,
                AssessmentId = dto.AssessmentId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                DeckId = dto.DeckId
            };
            await _repositoryWrapper.TraitifyRepository.Create(traitify);
            await _repositoryWrapper.TraitifyRepository.SaveAsync();
        }

        public async Task<TraitifyDto> CompleteAssessment(TraitifyDto dto)
        {
            Traitify traitify = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(dto.AssessmentId);
            traitify.CompleteDate = dto.CompleteDate;
            traitify.ResultData = dto.ResultData;
            traitify.ResultLength = dto.ResultLength;
            dto.Email = traitify.Email;
            await _repositoryWrapper.TraitifyRepository.SaveAsync();
            return dto;
        }
    }
}
