using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EducationalDegreeTypeFactory
    {
        public static EducationalDegreeType CreateEducationalDegreeType(string degreeType)
        {
            EducationalDegreeType rVal = new EducationalDegreeType();
            rVal.DegreeType = degreeType;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.EducationalDegreeTypeGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        public static async Task<EducationalDegreeType> GetOrDefault(IRepositoryWrapper repositoryWrapper, string degreeType)
        {
            degreeType = degreeType.Trim();

            EducationalDegreeType educationalDegreeType = await repositoryWrapper.EducationalDegreeTypeRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.DegreeType == degreeType)
                .FirstOrDefaultAsync();

            if (educationalDegreeType == null)
            {
                educationalDegreeType = await GetOrAdd(repositoryWrapper, Constants.NotSpecifedOption);
            }
            return educationalDegreeType;
        }


        public static async Task<EducationalDegreeType> GetOrAdd(IRepositoryWrapper repositoryWrapper, string degreeType)
        {
            degreeType = degreeType.Trim();

            EducationalDegreeType educationalDegreeType = await repositoryWrapper.EducationalDegreeTypeRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.DegreeType == degreeType)
                .FirstOrDefaultAsync();

            if (educationalDegreeType == null)
            {
                educationalDegreeType = CreateEducationalDegreeType(degreeType);
                await repositoryWrapper.EducationalDegreeTypeRepository.Create(educationalDegreeType);
                await repositoryWrapper.EducationalDegreeTypeRepository.SaveAsync();
            }
            return educationalDegreeType;
        }

        public static async Task<EducationalDegreeType> GetEducationalDegreeTypeByDegreeType(IRepositoryWrapper repositoryWrapper, string degreeType)
        {
            return await repositoryWrapper.EducationalDegreeTypeRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.DegreeType == degreeType)
                .FirstOrDefaultAsync();
        }
    }
}
