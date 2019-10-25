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
    public class PartnerTypeRepository : UpDiddyRepositoryBase<PartnerType>, IPartnerTypeRepository
    {

        public PartnerTypeRepository(UpDiddyDbContext dbContext) : base(dbContext) { }

        public async Task<PartnerType> GetPartnerTypeByName(string PartnerTypeName)
        {
            return await GetAll()
                .Where(p => p.IsDeleted == 0 && p.Name == PartnerTypeName)
                .FirstOrDefaultAsync();
        }
    }
}
