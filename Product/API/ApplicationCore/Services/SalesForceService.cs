using Hangfire;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using EntityTypeConst = UpDiddyLib.Helpers.Constants.EventType;

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

        public async Task AddToWaitList(SalesForceWaitListDto dto)
        {
           var waitList  =_mapper.Map<SalesForceWaitList>(dto);
            await _repositoryWrapper.SalesForceWaitListRepository.Create(waitList);
            await _repositoryWrapper.SalesForceWaitListRepository.SaveAsync();
        }

    }
}