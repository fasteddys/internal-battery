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
using System.Linq;
using Action = UpDiddyApi.Models.Action;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class ActionRepository : UpDiddyRepositoryBase<Models.Action>, IActionRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ActionRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Action> GetByNameAsync(string name)
        {
            return await (from a in _dbContext.Action
                          where a.Name == name
                          select a).FirstOrDefaultAsync();
        }
    }
}
