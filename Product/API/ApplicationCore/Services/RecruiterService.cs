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
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Authorization;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class RecruiterService : IRecruiterService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public RecruiterService(IConfiguration configuration, IRepositoryWrapper repositoryWrapper, IMapper mapper, IUserService userService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
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
                if (recruiterDto.IsInAuth0RecruiterGroupRecruiter)
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

                var getUserResponse = await _userService.GetUserByEmailAsync(recruiter.Email);
                if (getUserResponse.Success)
                {
                    bool isAssignedToRecruiterRole = getUserResponse.User.Roles.Contains(Role.Recruiter);

                    if (recruiterDto.IsInAuth0RecruiterGroupRecruiter && !isAssignedToRecruiterRole)
                    {
                        // assign permission
                        await this.AssignRecruiterPermissionsAsync(recruiterDto.SubscriberGuid);
                    }
                    else if (!recruiterDto.IsInAuth0RecruiterGroupRecruiter && isAssignedToRecruiterRole)
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
                if (recruiterDto.IsInAuth0RecruiterGroupRecruiter)
                {
                    var getUserResponse = await _userService.GetUserByEmailAsync(recruiter.Email);
                    if (!getUserResponse.Success || string.IsNullOrWhiteSpace(getUserResponse.User.UserId))
                        throw new ApplicationException("User could not be found in Auth0");
                    await _userService.RemoveRoleFromUserAsync(getUserResponse.User.UserId, Role.Recruiter);
                }
            }
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
                    recruiter.IsInAuth0RecruiterGroupRecruiter = true;
                else
                    recruiter.IsInAuth0RecruiterGroupRecruiter = false;
            }
        }

        #endregion
    }
}
