using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EducationalInstitutionFactory
    {
        static public EducationalInstitution CreateEducationalInstitution(string institutionName)
        {
            EducationalInstitution rVal = new EducationalInstitution();
            rVal.Name = institutionName;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.EducationalInstitutionGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public async Task<EducationalInstitution> GetOrAdd(IRepositoryWrapper repositoryWrapper, string institutionName)
        {

            institutionName = institutionName.Trim();

            EducationalInstitution educationalInstitution = await repositoryWrapper.EducationalInstitutionRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.Name == institutionName)
                .FirstOrDefaultAsync();

            if (educationalInstitution == null)
            {
                educationalInstitution = CreateEducationalInstitution(institutionName);
                await repositoryWrapper.EducationalInstitutionRepository.Create(educationalInstitution);
                await repositoryWrapper.EducationalInstitutionRepository.SaveAsync();
            }
            return educationalInstitution;
        }
    }
}
