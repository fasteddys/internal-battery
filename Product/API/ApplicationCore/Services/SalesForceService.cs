using AutoMapper;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SalesForceService : ISalesForceService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        private readonly IMapper _mapper;

        public SalesForceService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;

        }

        public async Task AddToWaitList(SalesForceSignUpListDto dto)
        {
            var item = _mapper.Map<SalesForceSignUpList>(dto);
            item.CreateDate = DateTime.UtcNow;
            item.SalesForceSignUpListGuid = Guid.NewGuid();
            await _repositoryWrapper.SalesForceSignUpListRepository.Create(item);
            await _repositoryWrapper.SalesForceSignUpListRepository.SaveAsync();
        }
    }
}