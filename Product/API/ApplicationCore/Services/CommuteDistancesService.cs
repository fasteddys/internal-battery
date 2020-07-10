using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;


namespace UpDiddyApi.ApplicationCore.Services
{
    public class CommuteDistancesService: ICommuteDistancesService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public CommuteDistancesService(
            IRepositoryWrapper repositoryWrapper, 
            ILogger<CommuteDistancesService> logger,
            IMapper mapper, 
            IConfiguration configuration) 
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
        }


        public async Task<CommuteDistancesDto> GetCommuteDistances(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            _logger.LogInformation($"CommuteDistancesService:GetCommuteDistances begin.");

            try
            {
                var commuteDistances = await _repositoryWrapper.CommuteDistancesRepository.GetCommuteDistances(limit, offset, sort, order);
                
                return _mapper.Map<CommuteDistancesDto>(commuteDistances); ;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CommuteDistancesService:GetCommuteDistances  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"CommuteDistancesService:GetCommuteDistances end.");

        }

        public async Task<CommuteDistanceDto> GetCommuteDistance(Guid commuteDistanceGuid)
        {
            _logger.LogInformation($"CommuteDistancesService:GetCommuteDistance begin.");
            if(commuteDistanceGuid.Equals(Guid.Empty)) throw new FailedValidationException("commuteDistanceGuid cannot be empty.");
            try
            {
                var CommuteDistance = await _repositoryWrapper.CommuteDistancesRepository.GetCommuteDistanceByGuid(commuteDistanceGuid);
                
                return _mapper.Map<CommuteDistanceDto>(CommuteDistance); ;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CommuteDistancesService:GetCommuteDistance  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"CommuteDistancesService:GetCommuteDistance end.");
        }
    }
}
