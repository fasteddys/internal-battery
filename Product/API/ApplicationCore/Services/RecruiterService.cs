using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Authorization;
using UpDiddyApi.Models;
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
            _recruiterGroupId = _configuration.GetSection("ADGroups:Values")
                                             .Get<List<ConfigADGroup>>().FirstOrDefault(g => g.Name == "Recruiter").Id;
        }

        public async Task<string> AddRecruiterAsync(RecruiterDto recruiterDto)
        {
            string response;
            //Assign Recruiter permissions to subscriber
            if (recruiterDto.SubscriberGuid != null && recruiterDto.SubscriberGuid != Guid.Empty)
            {
                //get subscriber using subscriber Guid
                var subscriber = await _repositoryWrapper.Subscriber.GetSubscriberByGuidAsync(recruiterDto.SubscriberGuid);

                //check if recruiter exist
                var queryableRecruiter = await _repositoryWrapper.RecruiterRepository.GetAllRecruiters();
                var existingRecruiter = await queryableRecruiter.Where(r => r.SubscriberId == subscriber.SubscriberId).FirstOrDefaultAsync(); ;

                if (existingRecruiter != null)
                {
                    if (existingRecruiter.IsDeleted == 1)
                    {
                        existingRecruiter.FirstName = recruiterDto.FirstName;
                        existingRecruiter.LastName = recruiterDto.LastName;
                        existingRecruiter.PhoneNumber = recruiterDto.PhoneNumber;
                        existingRecruiter.IsDeleted = 0; //activates the recruiter if he is deleted
                        existingRecruiter.ModifyDate = DateTime.Now;

                        await _repositoryWrapper.RecruiterRepository.UpdateRecruiter(existingRecruiter);
                    }
                    else
                    {
                        response = "Exist";
                        return response;
                    }
                }
                else
                {
                    MailAddress mailAddress = new MailAddress(recruiterDto.Email);
                    var host = mailAddress.Host.Split('.')[0];
                    //get CompanyId from email domain
                    var company = await _repositoryWrapper.Company.GetAllCompanies()
                                                    .Result.Where(c => c.IsDeleted == 0 && c.CompanyName.ToLower().Replace(" ", String.Empty) == host).FirstOrDefaultAsync();
                    if (company != null)
                    {
                        var newRecruiter = _mapper.Map<Recruiter>(recruiterDto);

                        //assign companyGuid
                        newRecruiter.RecruiterGuid = Guid.NewGuid();
                        newRecruiter.SubscriberId = subscriber.SubscriberId;
                        newRecruiter.CompanyId = company.CompanyId;
                        BaseModelFactory.SetDefaultsForAddNew(newRecruiter);

                        await _repositoryWrapper.RecruiterRepository.AddRecruiter(newRecruiter);
                    }
                    else
                    {
                        response = "Invalid";
                        return response;
                    }

                }


                //Assign permission to recruiter
                if(recruiterDto.IsInADRecruiterGroupRecruiter)
                    await AssignRecruiterPermissions(recruiterDto.SubscriberGuid);

                response = "Added";
            }
            else
            {
                response = "Invalid";
            }

            return response;

        }

        public async Task<List<RecruiterDto>> GetRecruitersAsync()
        {
            var queryableRecruiters = await _repositoryWrapper.RecruiterRepository.GetAllRecruiters();

            var includeDependentsToRecruiters = queryableRecruiters.Include<Recruiter>("Subscriber").Include<Recruiter>("Company");
            //get only non deleted records
            var recruiters = _mapper.Map<List<RecruiterDto>>(await includeDependentsToRecruiters.Where(c => c.IsDeleted == 0 && c.RecruiterGuid != Guid.Empty).ToListAsync());

            await CheckRecruiterPermissionAsync(recruiters);
            return recruiters;
        }

        public async Task EditRecruiterAsync(RecruiterDto recruiterDto)
        {
            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterByRecruiterGuid(recruiterDto.RecruiterGuid);
            if (recruiter != null)
            {
                recruiter.FirstName = recruiterDto.FirstName;
                recruiter.LastName = recruiterDto.LastName;
                recruiter.PhoneNumber = recruiterDto.PhoneNumber;
                recruiter.ModifyDate = DateTime.Now;

                await _repositoryWrapper.RecruiterRepository.UpdateRecruiter(recruiter);

                //check if member was assigned permission previously
                var members = await GetRecruiterGroupMembers();
                if (recruiterDto.IsInADRecruiterGroupRecruiter)
                {                   
                    if (members.FirstOrDefault(x => x["url"].ToString().Contains(recruiterDto.SubscriberGuid.ToString())) == null)
                    {
                        await AssignRecruiterPermissions(recruiterDto.SubscriberGuid);
                    }
                }
                else
                {
                    
                    if (members.FirstOrDefault(x => x["url"].ToString().Contains(recruiterDto.SubscriberGuid.ToString())) != null)
                    {
                        await RevokeRecruiterPermissions(recruiterDto.SubscriberGuid);
                    }
                }
                
            }
        }

        public async Task DeleteRecruiterAsync(RecruiterDto recruiterDto)
        {
            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterByRecruiterGuid(recruiterDto.RecruiterGuid);

            if (recruiter != null)
            {
                //set isDeleted to 1 to delete the record
                recruiter.IsDeleted = 1;
                recruiter.ModifyDate = DateTime.Now;

                await _repositoryWrapper.RecruiterRepository.UpdateRecruiter(recruiter);

                //check if member was assigned permission previously
                var members = await GetRecruiterGroupMembers();
                if (members.FirstOrDefault(x => x["url"].ToString().Contains(recruiterDto.SubscriberGuid.ToString())) != null)
                {
                    await RevokeRecruiterPermissions(recruiterDto.SubscriberGuid);
                }
            }
        }

        #region Private methods
        private async Task AssignRecruiterPermissions(Guid subscriberGuid)
        {
            var api = "/groups/" + _recruiterGroupId + "/$links/members";
            var url = $"https://graph.windows.net/" + _tenant + "/directoryObjects/" + subscriberGuid.ToString();

            var result = await _graphService.SendGraphPostRequest(api, JsonConvert.SerializeObject(new { url }));
        }

        private async Task RevokeRecruiterPermissions(Guid subscriberGuid)
        {
            var api = "/groups/" + _recruiterGroupId + "/$links/members/" + subscriberGuid.ToString();
            var result = await _graphService.SendGraphDeleteRequest(api);
        }

        private async Task CheckRecruiterPermissionAsync(List<RecruiterDto> recruiters)
        {
            var members = await GetRecruiterGroupMembers();

            foreach (var recruiter in recruiters)
            {
                if (members.FirstOrDefault(x => x["url"].ToString().Contains(recruiter.Subscriber.SubscriberGuid.ToString())) != null)
                    recruiter.IsInADRecruiterGroupRecruiter = true;
                else
                    recruiter.IsInADRecruiterGroupRecruiter = false;
            }

        }

        private async Task<List<JToken>> GetRecruiterGroupMembers()
        {
            //get all members of recruiter group
            var api = "/groups/" + _recruiterGroupId + "/$links/members/";
            var membersResponse = await _graphService.SendGraphGetRequest(api, "");

            var members = JObject.Parse(membersResponse)["value"].ToList();

            return members;
        }
        #endregion

    }
}
