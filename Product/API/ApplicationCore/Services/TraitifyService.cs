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
        public TraitifyService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
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
            Subscriber subscriber = await _repositoryWrapper.Subscriber.GetSubscriberByEmailAsync(dto.Email);
            Traitify traitify = new Traitify() {
                Subscriber = subscriber == null ? null : subscriber,
                AssessmentId = dto.AssessmentId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                DeckId = dto.DeckId
            };
            await _repositoryWrapper.TraitifyRepository.Create(traitify);
            await _repositoryWrapper.TraitifyRepository.SaveAsync();
        }
    }
}
