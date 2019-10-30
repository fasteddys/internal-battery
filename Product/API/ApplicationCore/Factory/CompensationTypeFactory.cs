using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CompensationTypeFactory
    {

        public static long AnnualCompensation(decimal compensation, CompensationType compensationType)
        {
            long rVal = (long)compensation;

            switch (compensationType.CompensationTypeName.ToLower())
            {
                case "hourly":
                    rVal *= 2080;
                    break;
                case "weekly":
                    rVal *= 52;
                    break;
                case "monthly":
                    rVal *= 12;
                    break;
                default:
                    break;
            }
            return rVal;
        }

        public static async Task<CompensationType> GetCompensationTypeByGuid(IRepositoryWrapper repositoryWrapper, Guid CompensationTypeGuid)
        {
            return await repositoryWrapper.CompensationTypeRepository.GetAll()
                .Where(s => s.IsDeleted == 0 && s.CompensationTypeGuid == CompensationTypeGuid)
                .FirstOrDefaultAsync();
        }

        public static async Task<CompensationType> GetCompensationTypeByName(IRepositoryWrapper repositoryWrapper, string CompensationTypeName)
        {
            return await repositoryWrapper.CompensationTypeRepository.GetAll()
                .Where(s => s.IsDeleted == 0 && s.CompensationTypeName == CompensationTypeName)
                .FirstOrDefaultAsync();
        }

        static public CompensationType CreateCompensationType(string CompensationTypeName)
        {
            CompensationType rVal = new CompensationType();
            rVal.CompensationTypeGuid = Guid.NewGuid();
            rVal.CompensationTypeName = CompensationTypeName;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.IsDeleted = 0;
            return rVal;
        }


        static public async Task<CompensationType> GetOrAdd(IRepositoryWrapper repositoryWrapper, string CompensationTypeName)
        {
            CompensationTypeName = CompensationTypeName.Trim();
            CompensationType compensatopnType = await repositoryWrapper.CompensationTypeRepository.GetAll()
                .Where(c => c.IsDeleted == 0 && c.CompensationTypeName == CompensationTypeName)
                .FirstOrDefaultAsync();

            if (compensatopnType == null)
            {
                compensatopnType = CreateCompensationType(CompensationTypeName);
                await repositoryWrapper.CompensationTypeRepository.Create(compensatopnType);
                await repositoryWrapper.CompensationTypeRepository.SaveAsync();
            }
            return compensatopnType;
        }

    }
}
