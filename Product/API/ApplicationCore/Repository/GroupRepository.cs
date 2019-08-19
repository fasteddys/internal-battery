using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class GroupRepository : UpDiddyRepositoryBase<Group>, IGroupRepository
    {
        UpDiddyDbContext _dbContext;

        public GroupRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Group> GetGroupByName(string Name)
        {
            var group= await GetByConditionAsync(g => g.Name.Equals(Name) && g.IsDeleted==0);
            return group.FirstOrDefault();
        }

        public async Task<Group> GetGroupByGuid(Guid groupGuid)
        {
            var group= await GetByConditionAsync(g => g.GroupGuid==groupGuid && g.IsDeleted==0);
            return group.FirstOrDefault();
        }

        public async Task<Group> GetDefaultGroup()
        {
            var defaultGroup=await GetByConditionAsync(g=>g.Name=="CareerCircle Organic Signup" && g.IsDeleted==0);
            return defaultGroup.FirstOrDefault();
        }
    }
}
