using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Helpers.Job;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Shared.GoogleJobs;
using Microsoft.AspNetCore.Http;


namespace UpDiddyApi.ApplicationCore.Services
{
    public class ServiceOfferingService : IServiceOfferingService 
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
      

        public ServiceOfferingService(IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {

            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;

        }

        public async Task<IList<ServiceOfferingDto>> GetAllServiceOfferings()
        {
             
                IList<ServiceOffering> serviceOfferings = await _repositoryWrapper.ServiceOfferingRepository.GetAllServiceOfferings();
                var rVal = _mapper.Map<List<ServiceOfferingDto>>(serviceOfferings);
                return rVal;                   
        }

    }
}
