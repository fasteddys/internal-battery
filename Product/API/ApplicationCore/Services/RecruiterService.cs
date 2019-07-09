using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Authorization;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class RecruiterService : IRecruiterService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IB2CGraph _graphService;
        private readonly IConfiguration _configuration;
        private readonly string _tenant;
        private readonly string _recruiterGroupId;
        public RecruiterService(IConfiguration configuration, IRepositoryWrapper repositoryWrapper, IMapper mapper, IB2CGraph graphService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _graphService = graphService;
            _configuration = configuration;
            _tenant = _configuration["AzureAdB2C:Tenant"];
            _recruiterGroupId= _configuration.GetSection("ADGroups:Values")
                                             .Get<List<ConfigADGroup>>().FirstOrDefault(g=>g.Name=="Recruiter").Id;
        }

        public async Task AddRecruiterAsync(RecruiterDto company)
        {

            //Assign Recruiter permissions to subscriber
            if (company.SubscriberGuid!=null && company.SubscriberGuid!=Guid.Empty)
            {
                if(company.isRecruiter)
                {
                    var api = "/groups/" + _recruiterGroupId + "/$links/members";
                    var url = $"https://graph.windows.net/" + _tenant + "/directoryObjects/"+ company.SubscriberGuid.ToString();

                    var result = await _graphService.SendGraphPostRequest(api, JsonConvert.SerializeObject(new { url }));
                }
                else
                {
                    var api = "/groups/" + _recruiterGroupId + "/$links/members/"+ company.SubscriberGuid.ToString();
                    var result = await _graphService.SendGraphDeleteRequest(api);
                }
              
            }
           

            //if(!string.IsNullOrEmpty(company.Email))
            //{
            //    _repositoryWrapper.RecruiterRepository.
            //}
    }

    public async Task<List<RecruiterDto>> GetRecruitersAsync()
    {
        var queryableRecruiters = await _repositoryWrapper.RecruiterRepository.GetAllRecruiters();
        //get only non deleted records
        return _mapper.Map<List<RecruiterDto>>(await queryableRecruiters.Where(c => c.IsDeleted == 0 && c.RecruiterGuid != Guid.Empty).ToListAsync());
    }
}
}
