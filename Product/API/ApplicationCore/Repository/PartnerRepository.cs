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

        public async Task<Partner> GetOrCreatePartnerByName ( string partnerName, PartnerType partnerType)
        {

 
            Partner Partner = await GetPartnerByName(partnerName);
            if (Partner == null)
            {
                Partner = new Partner()
                {
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = DateTime.UtcNow,
                    CreateGuid = Guid.NewGuid(),
                    ModifyGuid = Guid.NewGuid(),
                    PartnerGuid = Guid.NewGuid(),
                    IsDeleted = 0,
                    Name = partnerName,
                    PartnerTypeId = partnerType.PartnerTypeId,

                };
                await  Create(Partner);
                await SaveAsync();
            }

            return Partner;
        }

    }
}
