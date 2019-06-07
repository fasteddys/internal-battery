using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EducationalDegreeFactory
    {
        public static async Task<EducationalDegree> CreateEducationalDegree(string degree)
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

        static public async Task<EducationalDegree> GetOrAdd(UpDiddyDbContext db, string degree)
        {
            degree = degree.Trim();

            EducationalDegree educationalDegree = db.EducationalDegree
                .Where(s => s.IsDeleted == 0 && s.Degree == degree)
                .FirstOrDefault();

            if (educationalDegree == null)
            {
                educationalDegree =  await CreateEducationalDegree(degree);
                db.EducationalDegree.Add(educationalDegree);
                db.SaveChanges();
            }
            return educationalDegree;
        }

    }
}
