using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CommunityGroupFactory
    {
        public static async Task<CommunityGroup> GetCommunityGroup(IRepositoryWrapper repositoryWrapper, int communityGroupId)
        {
            return await repositoryWrapper.CommunityGroupRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.CommunityGroupId == communityGroupId)
                .FirstOrDefaultAsync();
        }

        public static async Task<CommunityGroupDto> GetCommunityGroup(IRepositoryWrapper repositoryWrapper, Guid communityGroupGuid, ILogger _syslog, IMapper _mapper)
        {
            if (Guid.Empty.Equals(communityGroupGuid))
            {
                _syslog.Log(LogLevel.Information, $"***** CommunityGroupFactory:GetCommunityGroup empty community group guid supplied.");
                return new CommunityGroupDto();
            }

            CommunityGroup communityGroup = await repositoryWrapper.CommunityGroupRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.CommunityGroupGuid == communityGroupGuid)
                .FirstOrDefaultAsync();

            CommunityGroupDto communityGroupDto = _mapper.Map<CommunityGroupDto>(communityGroup);

            return communityGroupDto;
        }

        public static async Task<CommunityGroup> GetCommunityGroup(IRepositoryWrapper repositoryWrapper, Guid communityGroupGuid)
        {
            return await repositoryWrapper.CommunityGroupRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.CommunityGroupGuid == communityGroupGuid)
                .FirstOrDefaultAsync();
        }


    }
   
}
