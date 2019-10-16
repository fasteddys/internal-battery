using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services.Identity.Communication;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyLib.Shared;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services.Identity
{
    public class UserService : IUserService
    {
        private const string _CACHE_KEY = "AUTH0_ACCESS_TOKEN";
        private const string _CONNECTION_TYPE = "Username-Password-Authentication";
        private Dictionary<string, string> _tokenParameters = null;
        private readonly string _domain = null;
        private readonly Uri _managementApiUrl = null;
        private readonly string _auth0CryptoKey = null;
        private readonly string _cryptoKey = null;
        private readonly IMemoryCache _memoryCache;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _logger;
        private readonly IB2CGraph _graphClient;
        private readonly string _acceptedTermsOfServiceKeyName = null;
        private readonly string _isOptInToMarketingEmailsKeyName = null;
        public UserService(IServiceProvider services)
        {
            var configuration = services.GetService<IConfiguration>();
            _tokenParameters = new Dictionary<string, string> {
                { "client_id", configuration["Auth0:ManagementApi:client_id"] },
                { "client_secret", configuration["Auth0:ManagementApi:client_secret"]},
                { "audience", configuration["Auth0:ManagementApi:audience"] },
                { "grant_type", configuration["Auth0:ManagementApi:grant_type"]}
            };
            _domain = configuration["Auth0:Domain"];
            _managementApiUrl = new Uri(configuration["Auth0:ManagementApi:url"]);
            _auth0CryptoKey = configuration["Auth0:CryptoKey"];
            _memoryCache = services.GetService<IMemoryCache>();
            _repositoryWrapper = services.GetService<IRepositoryWrapper>();
            _logger = services.GetService<ILogger<UserService>>();
            _graphClient = services.GetService<IB2CGraph>();
            _acceptedTermsOfServiceKeyName = configuration["AzureAdB2C:ExtensionFields:AgreeToCareerCircleTerms"];
            _isOptInToMarketingEmailsKeyName = configuration["AzureAdB2C:ExtensionFields:AgreeToCareerCircleMarketing"];
        }

        private async Task ClearApiTokenAsync()
        {
            _memoryCache.Remove(_CACHE_KEY);
        }

        private async Task<string> GetApiTokenAsync()
        {
            string encryptedToken = null;

            if (!_memoryCache.TryGetValue<string>(_CACHE_KEY, out encryptedToken))
            {
                string apiResponse = null;
                using (var client = new HttpClient())
                {
                    var encodedContent = new FormUrlEncodedContent(_tokenParameters);
                    using (var response = await client.PostAsync(_managementApiUrl, encodedContent))
                    {
                        apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }
                JObject json = JObject.Parse(apiResponse);
                encryptedToken = Crypto.Encrypt(_auth0CryptoKey, (string)json["access_token"]);
                using (var entry = _memoryCache.CreateEntry(_CACHE_KEY))
                {
                    entry.Value = encryptedToken;
                    entry.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
                }
            }

            return Crypto.Decrypt(_auth0CryptoKey, encryptedToken);
        }

        public async Task<GetUserResponse> GetUserByEmailAsync(string email)
        {
            User user = null;
            var apiToken = await GetApiTokenAsync();
            var managementApiClient = new ManagementApiClient(apiToken, _domain);
            IList<Auth0.ManagementApi.Models.User> users = null;
            try
            {
                users = await managementApiClient.Users.GetUsersByEmailAsync(email);
            }
            catch (ApiException ae)
            {
                if (ae.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning($"An unauthorized Auth0 ApiException occurred in UserService.GetUserByEmailAsync (will refresh token and retry one time): {ae.Message}", ae);

                    try
                    {
                        // clear the token, get a new one, and try one more time
                        await ClearApiTokenAsync();
                        apiToken = await GetApiTokenAsync();
                        managementApiClient = new ManagementApiClient(apiToken, _domain);
                        users = await managementApiClient.Users.GetUsersByEmailAsync(email);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"An unexpected exception occurred in UserService.GetUserByEmailAsync (will not be retried): {e.Message}", e);
                        return new GetUserResponse(false, "An unexpected error has occured.", null);
                    }
                }
                else
                {
                    return new GetUserResponse(false, ae.Message, null);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected exception occurred in UserService.GetUserByEmailAsync (will not be retried): {e.Message}", e);
                return new GetUserResponse(false, "An unexpected error has occured.", null);
            }

            try
            {
                if (users != null && users.Count()> 0)
                {
                    var auth0User = users.FirstOrDefault();
                    user = new User()
                    {
                        Email = auth0User.Email,
                        SubscriberGuid = auth0User.AppMetadata.subscriberGuid,
                        UserId = auth0User.UserId,
                        EmailVerified = auth0User.EmailVerified
                    };
                }
                else
                {
                    return new GetUserResponse(false, "No user was found with that email address.", null);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred while retrieving the user in UserService.GetUserByEmailAsync (will not be retried): {e.Message}", e);
                return new GetUserResponse(false, "An error occurred while retrieving the user", null);
            }

            return new GetUserResponse(true, "User was retrieved successfully.", user);
        }

        public async Task<CreateUserResponse> CreateUserAsync(User user, bool requireEmailVerification, params Role[] userRoles)
        {
            Auth0.ManagementApi.Models.User userCreationResponse = null;
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByEmailAsync(user.Email);

            if (subscriber != null)
            {
                return new CreateUserResponse(false, "Email already in use.", null);
            }

            var apiToken = await GetApiTokenAsync();
            var managementApiClient = new ManagementApiClient(apiToken, _domain);

            UserCreateRequest userCreationRequest = new UserCreateRequest()
            {
                Email = user.Email,
                Connection = _CONNECTION_TYPE,
                Password = user.Password,
                VerifyEmail = requireEmailVerification,
                AppMetadata = new JObject()
            };
            Guid subscriberGuid = Guid.NewGuid();
            userCreationRequest.AppMetadata.subscriberGuid = subscriberGuid;
            userCreationRequest.AppMetadata.acceptedTermsOfService = "October 16, 2018, version 1.0"; // may need to make this dynamic later
            userCreationRequest.AppMetadata.isOptInToMarketingEmails = user.IsAgreeToMarketingEmails;
            try
            {
                userCreationResponse = await managementApiClient.Users.CreateAsync(userCreationRequest);
            }
            catch (ApiException ae)
            {
                if (ae.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning($"An unauthorized Auth0 ApiException occurred in UserService.CreateUserAsync (will refresh token and retry one time): {ae.Message}", ae);

                    try
                    {
                        // clear the token, get a new one, and try one more time
                        await ClearApiTokenAsync();
                        apiToken = await GetApiTokenAsync();
                        managementApiClient = new ManagementApiClient(apiToken, _domain);
                        userCreationResponse = await managementApiClient.Users.CreateAsync(userCreationRequest);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"An unexpected exception occurred in UserService.CreateUserAsync (will not be retried): {e.Message}", e);
                        return new CreateUserResponse(false, "An unexpected error has occured.", null);
                    }
                }
                else
                {
                    return new CreateUserResponse(false, ae.Message, null);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected exception occurred in UserService.CreateUserAsync (will not be retried): {e.Message}", e);
                return new CreateUserResponse(false, "An unexpected error has occured.", null);
            }

            user.UserId = userCreationResponse.UserId;
            user.SubscriberGuid = subscriberGuid;

            return new CreateUserResponse(true, "Account has been created.", user);
        }

        public async Task<CreateUserResponse> MigrateUserAsync(User user)
        {
            Auth0.ManagementApi.Models.User userCreationResponse = null;
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByEmailAsync(user.Email);

            if (subscriber == null)
            {
                return new CreateUserResponse(false, "The subscriber does not exist.", null);
            }
            else
            {
                var userGroupsFromADB2C = await _graphClient.GetUserGroupsByObjectId(subscriber.SubscriberGuid.Value.ToString());
                var adb2cAccount = await _graphClient.GetUserBySignInEmail(user.Email);
                string acceptedTermsOfService = string.Empty;
                bool isOptInToMarketingEmails = false;
                if (adb2cAccount.AdditionalData.Keys.Contains(_acceptedTermsOfServiceKeyName))
                    acceptedTermsOfService = adb2cAccount.AdditionalData[_acceptedTermsOfServiceKeyName].ToString();
                if (adb2cAccount.AdditionalData.Keys.Contains(_isOptInToMarketingEmailsKeyName))
                    isOptInToMarketingEmails = bool.Parse(adb2cAccount.AdditionalData[_isOptInToMarketingEmailsKeyName].ToString());
                var apiToken = await GetApiTokenAsync();
                var managementApiClient = new ManagementApiClient(apiToken, _domain);

                UserCreateRequest userCreationRequest = new UserCreateRequest()
                {
                    Email = user.Email,
                    Connection = _CONNECTION_TYPE,
                    Password = user.Password,
                    VerifyEmail = !subscriber.IsVerified,
                    AppMetadata = new JObject()
                };
                userCreationRequest.AppMetadata.subscriberGuid = subscriber.SubscriberGuid;
                userCreationRequest.AppMetadata.acceptedTermsOfService = acceptedTermsOfService;
                userCreationRequest.AppMetadata.isOptInToMarketingEmails = isOptInToMarketingEmails;
                
                try
                {
                    userCreationResponse = await managementApiClient.Users.CreateAsync(userCreationRequest);
                    if (subscriber.IsVerified)
                        await managementApiClient.Users.UpdateAsync(userCreationResponse.UserId, new UserUpdateRequest() { EmailVerified = true });
                    if (userGroupsFromADB2C != null && userGroupsFromADB2C.Count() > 0)
                    {
                        List<string> newRoles = new List<string>();
                        var auth0Roles = await managementApiClient.Roles.GetAllAsync(new GetRolesRequest() { NameFilter = null });
                        foreach (var oldGroup in userGroupsFromADB2C)
                        {
                            var matchingRole = auth0Roles.Where(r => r.Name == oldGroup.DisplayName).FirstOrDefault();
                            newRoles.Add(matchingRole.Id);
                        }
                        if (newRoles.Count() > 0)
                            await managementApiClient.Users.AssignRolesAsync(userCreationResponse.UserId, new AssignRolesRequest() { Roles = newRoles.ToArray() });
                    }
                }
                catch (ApiException ae)
                {
                    if (ae.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning($"An unauthorized Auth0 ApiException occurred in UserService.MigrateUserAsync (will refresh token and retry one time): {ae.Message}", ae);

                        try
                        {
                            // clear the token, get a new one, and try one more time
                            await ClearApiTokenAsync();
                            apiToken = await GetApiTokenAsync();
                            managementApiClient = new ManagementApiClient(apiToken, _domain);
                            userCreationResponse = await managementApiClient.Users.CreateAsync(userCreationRequest);
                            if (subscriber.IsVerified)
                                await managementApiClient.Users.UpdateAsync(userCreationResponse.UserId, new UserUpdateRequest() { EmailVerified = true });
                            if (userGroupsFromADB2C != null && userGroupsFromADB2C.Count() > 0)
                            {
                                List<string> newRoles = new List<string>();
                                var auth0Roles = await managementApiClient.Roles.GetAllAsync(new GetRolesRequest() { NameFilter = null });
                                foreach (var oldGroup in userGroupsFromADB2C)
                                {
                                    var matchingRole = auth0Roles.Where(r => r.Name == oldGroup.DisplayName).FirstOrDefault();
                                    newRoles.Add(matchingRole.Id);
                                }
                                if (newRoles.Count() > 0)
                                    await managementApiClient.Users.AssignRolesAsync(userCreationResponse.UserId, new AssignRolesRequest() { Roles = newRoles.ToArray() });
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"An unexpected exception occurred in UserService.MigrateUserAsync (will not be retried): {e.Message}", e);
                            return new CreateUserResponse(false, "An unexpected error has occured.", null);
                        }
                    }
                    else
                    {
                        return new CreateUserResponse(false, ae.Message, null);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"An unexpected exception occurred in UserService.MigrateUserAsync (will not be retried): {e.Message}", e);
                    return new CreateUserResponse(false, "An unexpected error has occured.", null);
                }

                user.UserId = userCreationResponse.UserId;
                user.SubscriberGuid = userCreationResponse.AppMetadata.subscriberGuid;
                _graphClient.DisableUser(user.SubscriberGuid);

                return new CreateUserResponse(true, "User has been migrated successfully.", user);
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            var apiToken = await GetApiTokenAsync();
            var managementApiClient = new ManagementApiClient(apiToken, _domain);
            try
            {
                await managementApiClient.Users.DeleteAsync(userId);
            }
            catch (ApiException ae)
            {
                _logger.LogWarning($"An Auth0 ApiException occurred in UserService.DeleteUserAsync (will refresh token and retry one time): {ae.Message}", ae);
                if (ae.StatusCode == HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        // clear the token, get a new one, and try one more time
                        await ClearApiTokenAsync();
                        apiToken = await GetApiTokenAsync();
                        managementApiClient = new ManagementApiClient(apiToken, _domain);
                        await managementApiClient.Users.DeleteAsync(userId);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"An unexpected exception occurred in UserService.DeleteUserAsync (will not be retried): {e.Message}", e);
                    }
                }
            }
        }
    }
}
