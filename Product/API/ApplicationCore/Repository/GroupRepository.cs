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

        public Group GetGroupByName(string Name)
        {
            return _dbContext.Group.Where(g => g.Name.Equals(Name)).FirstOrDefault();
        }
    }
}
