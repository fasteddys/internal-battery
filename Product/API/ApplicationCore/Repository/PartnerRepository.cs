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

    public class PartnerRepository : UpDiddyRepositoryBase<Partner>, IPartnerRepository
    {
        public PartnerRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
        public async Task<Partner> GetPartnerByName(string PartnerName)
        {
            return await GetAll()
                .Where(p => p.IsDeleted == 0 && p.Name == PartnerName)
                .FirstOrDefaultAsync();
        }

        public async Task<Partner> GetPartnerByGuid(Guid partnerGuid)
        {
            return await GetAll()
                .Where(p => p.IsDeleted == 0 && p.PartnerGuid == partnerGuid)
                .FirstOrDefaultAsync();
        }
    }
}
