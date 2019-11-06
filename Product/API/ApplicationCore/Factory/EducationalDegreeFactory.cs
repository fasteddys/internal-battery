using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EducationalDegreeFactory
    {
        public static EducationalDegree CreateEducationalDegree(string degree)
        {
            EducationalDegree rVal = new EducationalDegree();
            rVal.Degree = degree;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.EducationalDegreeGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        public static async Task<EducationalDegree> GetOrAdd(IRepositoryWrapper repositoryWrapper, string degree)
        {
            degree = degree.Trim();

            EducationalDegree educationalDegree = await repositoryWrapper.EducationalDegreeRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.Degree == degree)
                .FirstOrDefaultAsync();

            if (educationalDegree == null)
            {
                educationalDegree =  CreateEducationalDegree(degree);
                await repositoryWrapper.EducationalDegreeRepository.Create(educationalDegree);
                await repositoryWrapper.EducationalDegreeRepository.SaveAsync();
            }
            return educationalDegree;
        }
 

    }
}
