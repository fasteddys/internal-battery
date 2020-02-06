using AutoMapper;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Authorization;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class RecruiterService : IRecruiterService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IAzureSearchService _azureSearchService;

        public RecruiterService(IConfiguration configuration, IRepositoryWrapper repositoryWrapper, IMapper mapper, IUserService userService, IAzureSearchService azureSearchService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
            _azureSearchService = azureSearchService;
        }

        public async Task<string> AddRecruiterAsync(RecruiterDto recruiterDto)
        {
            string response;
            //Assign Recruiter permissions to subscriber
            if (recruiterDto.SubscriberGuid != null && recruiterDto.SubscriberGuid != Guid.Empty)
            {
                //get subscriber using subscriber Guid
                var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(recruiterDto.SubscriberGuid);

                //check if recruiter exist
                var queryableRecruiter = _repositoryWrapper.RecruiterRepository.GetAllRecruiters();
                var existingRecruiter = await queryableRecruiter.Where(r => r.SubscriberId == subscriber.SubscriberId).FirstOrDefaultAsync();

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
                                        .Where(c => c.IsDeleted == 0 && c.CompanyName.ToLower().Replace(" ", String.Empty) == host).FirstOrDefaultAsync();
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
                if (recruiterDto.IsInAuth0RecruiterGroup)
                    await AssignRecruiterPermissionsAsync(recruiterDto.SubscriberGuid);

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
            var queryableRecruiters = _repositoryWrapper.RecruiterRepository.GetAllRecruiters();

            var includeDependentsToRecruiters = queryableRecruiters.Include<Recruiter>("Subscriber").Include<Recruiter>("Company");
            //get only non deleted records
            var recruiters = _mapper.Map<List<RecruiterDto>>(await includeDependentsToRecruiters.Where(c => c.IsDeleted == 0
                                                                            && c.SubscriberId != null && c.CompanyId != null && c.RecruiterGuid != Guid.Empty).ToListAsync());

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



                // Assign autho permissions 
                var getUserResponse = await _userService.GetUserByEmailAsync(recruiter.Email);
                if (getUserResponse.Success)
                {
                    bool isAssignedToRecruiterRole = getUserResponse.User.Roles.Contains(Role.Recruiter);

                    if (recruiterDto.IsInAuth0RecruiterGroup && !isAssignedToRecruiterRole)
                    {
                        // assign permission
                        await this.AssignRecruiterPermissionsAsync(recruiterDto.SubscriberGuid);
                    }
                    else if (!recruiterDto.IsInAuth0RecruiterGroup && isAssignedToRecruiterRole)
                    {
                        // remove permission
                        await this.RevokeRecruiterPermissionsAsync(recruiterDto.SubscriberGuid);
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
                if (recruiterDto.IsInAuth0RecruiterGroup)
                {
                    var getUserResponse = await _userService.GetUserByEmailAsync(recruiter.Email);
                    if (!getUserResponse.Success || string.IsNullOrWhiteSpace(getUserResponse.User.UserId))
                        throw new ApplicationException("User could not be found in Auth0");
                    await _userService.RemoveRoleFromUserAsync(getUserResponse.User.UserId, Role.Recruiter);
                }
            }
        }




        public async Task<RecruiterInfoListDto> GetRecruiters(int limit, int offset, string sort, string order)
        {
            List<RecruiterInfoDto> rVal = null;
            rVal = await _repositoryWrapper.StoredProcedureRepository.GetRecruiters(limit, offset, sort, order);
            return _mapper.Map<RecruiterInfoListDto>(rVal);
        }


        public async Task<Guid> AddRecruiterAsync(RecruiterInfoDto recruiterDto)
        {

            // Do validations             
            if (recruiterDto.SubscriberGuid == null)
                throw new FailedValidationException("Subscriber must be specified");

            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(recruiterDto.SubscriberGuid.Value);
            if (subscriber == null)
                throw new FailedValidationException($"{recruiterDto.SubscriberGuid.Value} is not a valid subscriber");

            if (recruiterDto.CompanyGuid == null)
                throw new FailedValidationException("Company must be specified");

            var company = await _repositoryWrapper.Company.GetByGuid(recruiterDto.CompanyGuid.Value);
            if (company == null)
                throw new FailedValidationException($"{recruiterDto.CompanyGuid.Value} is not a valid company");

            //check if recruiter exist
            var queryableRecruiter = _repositoryWrapper.RecruiterRepository.GetAllRecruiters();
            var existingRecruiter = await queryableRecruiter.Where(r => r.SubscriberId == subscriber.SubscriberId).FirstOrDefaultAsync();
            Guid recruiterGuid;
            if (existingRecruiter != null)
            {
                if (existingRecruiter.IsDeleted == 1)
                {
                    recruiterGuid = existingRecruiter.RecruiterGuid;
                    existingRecruiter.FirstName = recruiterDto.FirstName;
                    existingRecruiter.LastName = recruiterDto.LastName;
                    existingRecruiter.PhoneNumber = recruiterDto.PhoneNumber;
                    existingRecruiter.IsDeleted = 0; //activates the recruiter if he is deleted
                    existingRecruiter.ModifyDate = DateTime.Now;
                    await _repositoryWrapper.RecruiterRepository.UpdateRecruiter(existingRecruiter);

                    // Update recruiter in azure
                    _azureSearchService.AddOrUpdateRecruiter(existingRecruiter);
                }
                else
                    throw new FailedValidationException($"Subscriber {subscriber.SubscriberGuid} is already a Recruiter");
            }
            else
            {
                recruiterGuid = Guid.NewGuid();
                var newRecruiter = new Recruiter()
                {

                    FirstName = recruiterDto.FirstName,
                    LastName = recruiterDto.LastName,
                    PhoneNumber = recruiterDto.PhoneNumber,
                    Email = recruiterDto.Email,
                    RecruiterGuid = recruiterGuid,
                    SubscriberId = subscriber.SubscriberId,
                    CompanyId = company.CompanyId
                };
                //assign companyGuid
                BaseModelFactory.SetDefaultsForAddNew(newRecruiter);
                await _repositoryWrapper.RecruiterRepository.AddRecruiter(newRecruiter);
                // add recruiter in azure
                _azureSearchService.AddOrUpdateRecruiter(newRecruiter);

            }
            //Assign permission to recruiter
            if (recruiterDto.IsInAuth0RecruiterGroup != null && recruiterDto.IsInAuth0RecruiterGroup.Value == true)
                await AssignRecruiterPermissionsAsync(recruiterDto.SubscriberGuid.Value);

            return recruiterGuid;
        }


        public async Task EditRecruiterAsync(RecruiterInfoDto recruiterDto, Guid Recruiter)
        {
            if ( Recruiter != recruiterDto.RecruiterGuid)
                throw new FailedValidationException($"Recruiter specified in URL does not match recruiter specified in body");


            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterByRecruiterGuid(recruiterDto.RecruiterGuid);
            if (recruiter == null)
                throw new FailedValidationException($"{recruiterDto.RecruiterGuid} is not a valid recruiter");

            if (recruiterDto.SubscriberGuid == null)
                throw new FailedValidationException("Subscriber must be specified");

            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(recruiterDto.SubscriberGuid.Value);
            if (subscriber == null)
                throw new FailedValidationException($"{recruiterDto.SubscriberGuid.Value} is not a valid subscriber");


            if (recruiterDto.CompanyGuid == null)
                throw new FailedValidationException($"Recruiters company is not specified");

            // validate that the update specifies a valid company
            var company = await _repositoryWrapper.Company.GetByGuid(recruiterDto.CompanyGuid.Value);
            if (company == null)
                throw new FailedValidationException($"{recruiterDto.CompanyGuid.Value} is not a valid company");

            if (company.CompanyId != recruiter.CompanyId)
                recruiter.CompanyId = company.CompanyId;

            recruiter.FirstName = recruiterDto.FirstName;
            recruiter.LastName = recruiterDto.LastName;
            recruiter.PhoneNumber = recruiterDto.PhoneNumber;
            recruiter.ModifyDate = DateTime.Now;
            recruiter.Email = recruiterDto.Email;

            await _repositoryWrapper.RecruiterRepository.UpdateRecruiter(recruiter);

            // Update recruiter in azure
            _azureSearchService.AddOrUpdateRecruiter(recruiter);

            if (recruiterDto.IsInAuth0RecruiterGroup != null)
            {
                var getUserResponse = await _userService.GetUserByEmailAsync(recruiter.Email);
                if (getUserResponse.Success)
                {
                    bool isAssignedToRecruiterRole = getUserResponse.User.Roles.Contains(Role.Recruiter);

                    if (recruiterDto.IsInAuth0RecruiterGroup.Value && !isAssignedToRecruiterRole)
                    {
                        // assign permission
                        await this.AssignRecruiterPermissionsAsync(recruiterDto.SubscriberGuid.Value);
                    }
                    else if (!recruiterDto.IsInAuth0RecruiterGroup.Value && isAssignedToRecruiterRole)
                    {
                        // remove permission
                        await this.RevokeRecruiterPermissionsAsync(recruiterDto.SubscriberGuid.Value);
                    }
                }
            }


        }

        public async Task DeleteRecruiterAsync(Guid SubscriberGuid, Guid RecruiterGuid)
        {
            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterByRecruiterGuid(RecruiterGuid);
            if (recruiter == null)
                throw new FailedValidationException($"{RecruiterGuid} is not a valid recruiter");
            // validate the subscriber to just be extra sure 
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(SubscriberGuid);
            if (subscriber == null)
                throw new FailedValidationException($"{SubscriberGuid} is not a valid subscriber");

            //set isDeleted to 1 to delete the record
            recruiter.IsDeleted = 1;
            recruiter.ModifyDate = DateTime.Now;
            recruiter.ModifyGuid = SubscriberGuid;

            await _repositoryWrapper.RecruiterRepository.UpdateRecruiter(recruiter);

            // Update recruiter in azure
            _azureSearchService.DeleteRecruiter(recruiter);

            // delete the user's permissions  
            var getUserResponse = await _userService.GetUserByEmailAsync(recruiter.Email);
            if (getUserResponse.Success && getUserResponse.User != null && getUserResponse.User.UserId != null && string.IsNullOrWhiteSpace(getUserResponse.User.UserId) == false)
                await _userService.RemoveRoleFromUserAsync(getUserResponse.User.UserId, Role.Recruiter);

        }

        public async Task<RecruiterInfoDto> GetRecruiterAsync(Guid RecruiterGuid)
        {
            var recruiter = _repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Include(c => c.Company)
                .Where(r => r.IsDeleted == 0 && r.RecruiterGuid == RecruiterGuid)
                .FirstOrDefault();

            if (recruiter == null)
                throw new FailedValidationException($"{RecruiterGuid} is not a valid recruiter");

            RecruiterInfoDto rVal = _mapper.Map<RecruiterInfoDto>(recruiter);
            rVal.IsInAuth0RecruiterGroup = await CheckRecruiterPermissionAsync(recruiter);

            return rVal;
        }


        public async Task<RecruiterInfoDto> GetRecruiterBySubscriberAsync(Guid SubscriberGuid)
        {
            var recruiter = _repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Include(c => c.Company)
                .Where(r => r.IsDeleted == 0 && r.Subscriber.SubscriberGuid == SubscriberGuid)
                .FirstOrDefault();

            if (recruiter == null)
                throw new FailedValidationException($"The subscriber {SubscriberGuid} is not a recruiter");

            RecruiterInfoDto rVal = _mapper.Map<RecruiterInfoDto>(recruiter);
            rVal.IsInAuth0RecruiterGroup = await CheckRecruiterPermissionAsync(recruiter);

            return rVal;
        }



        public async Task<RecruiterSearchResultDto> SearchRecruitersAsync(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", string companyName = "")
        {
            DateTime startSearch = DateTime.Now;
            RecruiterSearchResultDto searchResults = new RecruiterSearchResultDto();

            string searchServiceName = _configuration["AzureSearch:SearchServiceName"];
            string adminApiKey = _configuration["AzureSearch:SearchServiceQueryApiKey"];
            string recruiterIndexName = _configuration["AzureSearch:RecruiterIndexName"];

            // map descending to azure search sort syntax of "asc" or "desc"  default is ascending so only map descending 
            string orderBy = sort;
            if (order == "descending")
                orderBy = orderBy + " desc";
            List<String> orderByList = new List<string>();
            orderByList.Add(orderBy);

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            // Get recruiter search index 
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(recruiterIndexName);

            SearchParameters parameters;
            DocumentSearchResult<RecruiterInfoDto> results;

            parameters =
                new SearchParameters()
                {
                    Top = limit,
                    Skip = offset,
                    OrderBy = orderByList,
                    IncludeTotalResultCount = true,
                };


            //todo implement case insensitive filtering if it becomes necessary - strategry would be to index the companyname as lowercase into a filter field
            if (companyName != "")
                parameters.Filter = $"CompanyName eq '{companyName}'"; 

            results = indexClient.Documents.Search<RecruiterInfoDto>(keyword, parameters);

            DateTime startMap = DateTime.Now;
            searchResults.Recruiters = results?.Results?
                .Select(s => (RecruiterInfoDto)s.Document)
                .ToList();

            searchResults.TotalHits = results.Count.Value;
            searchResults.PageSize = limit;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.RecruiterCount = searchResults.Recruiters.Count;
            searchResults.PageNum = (offset / limit) + 1;

            DateTime stopMap = DateTime.Now;

            // calculate search timing metrics 
            TimeSpan intervalTotalSearch = stopMap - startSearch;
            TimeSpan intervalSearchTime = startMap - startSearch;
            TimeSpan intervalMapTime = stopMap - startMap;

            // assign search metrics to search results 
            searchResults.SearchTimeInMilliseconds = intervalTotalSearch.TotalMilliseconds;
            searchResults.SearchQueryTimeInTicks = intervalSearchTime.Ticks;
            searchResults.SearchMappingTimeInTicks = intervalMapTime.Ticks;
            return searchResults;
        }



        #region Private methods

        private async Task AssignRecruiterPermissionsAsync(Guid subscriberGuid)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            var getUserResponse = await _userService.GetUserByEmailAsync(subscriber.Email);
            if (!getUserResponse.Success || string.IsNullOrWhiteSpace(getUserResponse.User.UserId))
                throw new ApplicationException("User could not be found in Auth0");
            _userService.AssignRoleToUserAsync(getUserResponse.User.UserId, Role.Recruiter);
        }

        private async Task RevokeRecruiterPermissionsAsync(Guid subscriberGuid)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            var getUserResponse = await _userService.GetUserByEmailAsync(subscriber.Email);
            if (!getUserResponse.Success || string.IsNullOrWhiteSpace(getUserResponse.User.UserId))
                throw new ApplicationException("User could not be found in Auth0");
            _userService.RemoveRoleFromUserAsync(getUserResponse.User.UserId, Role.Recruiter);
        }

        private async Task CheckRecruiterPermissionAsync(List<RecruiterDto> recruiters)
        {
            var response = await _userService.GetUsersInRoleAsync(Role.Recruiter);
            if (!response.Success)
                throw new ApplicationException("There was a problem retrieving users in the recruiter role");
            var usersInRole = response.Users;
            foreach (var recruiter in recruiters)
            {
                var user = usersInRole.Where(u => u.Email == recruiter.Email).FirstOrDefault();
                if (user != null)
                    recruiter.IsInAuth0RecruiterGroup = true;
                else
                    recruiter.IsInAuth0RecruiterGroup = false;
            }
        }


        private async Task<bool> CheckRecruiterPermissionAsync(Recruiter recruiter)
        {
            var response = await _userService.GetUsersInRoleAsync(Role.Recruiter);
            if (!response.Success)
                throw new ApplicationException("There was a problem retrieving users in the recruiter role");
            var usersInRole = response.Users;

            var user = usersInRole.Where(u => u.Email == recruiter.Email).FirstOrDefault();
            if (user != null)
                return true;
            else
                return false;

        }






        #endregion
    }
}
