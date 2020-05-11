using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.B2B;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class HiringManagerRepository : UpDiddyRepositoryBase<HiringManager>, IHiringManagerRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public HiringManagerRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HiringManager> GetHiringManagerBySubscriberId(int SubscriberId)
        {
            var hiringManager = await _dbContext.HiringManager.Where(hm => hm.SubscriberId == SubscriberId && hm.IsDeleted == 0)
                .Include(hm => hm.Company)
                .Include(hm => hm.Company.Industry)
                .FirstOrDefaultAsync();

            return hiringManager;
        }

        public async Task AddHiringManager(int subscriberId)
        {
            _dbContext.HiringManager.Add(new HiringManager
            {
                SubscriberId = subscriberId,
                HiringManagerGuid = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow,
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateHiringManager(int subscriberId, HiringManagerDto hiringManagerDto)
        {
            var hiringManager = _dbContext.HiringManager
                .Where(hm => hm.SubscriberId == subscriberId)
                .Include(hm => hm.Subscriber)
                .Include(hm => hm.Subscriber.State)
                .Include(hm => hm.Company)
                .Include(hm => hm.Company.Industry)
                .FirstOrDefault();

            //add/update Company details

            var industry = _dbContext.Industry.FirstOrDefault(i => hiringManagerDto.CompanyIndustryGuid.HasValue && i.IndustryGuid == hiringManagerDto.CompanyIndustryGuid.Value);
            var state = _dbContext.State.FirstOrDefault(s => s.StateGuid == hiringManagerDto.StateGuid);
            if (hiringManager.Company == null)
            {
                hiringManager.Company = new Company
                {
                    CompanyGuid = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.NewGuid(),
                    WebsiteUrl = hiringManagerDto.CompanyWebsiteUrl,
                    CompanyName = hiringManagerDto.CompanyName,
                    EmployeeSize = hiringManagerDto.CompanySize,
                    Description = hiringManagerDto.CompanyDescription,
                    Industry = industry
                };
            }
            else
            {
                hiringManager.Company.CompanyName = hiringManagerDto.CompanyName;
                hiringManager.Company.EmployeeSize = hiringManagerDto.CompanySize;
                hiringManager.Company.WebsiteUrl = hiringManagerDto.CompanyWebsiteUrl;
                hiringManager.Company.Description = hiringManagerDto.CompanyDescription;
                hiringManager.Company.ModifyGuid = Guid.NewGuid();
                hiringManager.Company.ModifyDate = DateTime.UtcNow;
                hiringManager.Company.Industry = industry;
            }

            //update subscribers record
            hiringManager.Subscriber.FirstName = hiringManagerDto.FirstName;
            hiringManager.Subscriber.LastName = hiringManagerDto.LastName;
            hiringManager.Subscriber.City = hiringManagerDto.City;
            hiringManager.Subscriber.State = state;
            hiringManager.Subscriber.ModifyDate = DateTime.UtcNow;
            hiringManager.Subscriber.ModifyGuid = Guid.NewGuid();
            hiringManager.Subscriber.Title = hiringManagerDto.Title;
            hiringManager.Subscriber.PhoneNumber = hiringManagerDto.PhoneNumber;


            //update HiringManager record
            hiringManager.HardToFindFillSkillsRoles = hiringManagerDto.HardToFindFillSkillsRoles;
            hiringManager.SkillsRolesWeAreAlwaysHiringFor = hiringManagerDto.SkillsRolesWeAreAlwaysHiringFor;


            await _dbContext.SaveChangesAsync();
        }

    }
}
