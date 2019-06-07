using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

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

        public static async Task<EducationalDegreeType> GetOrDefault(UpDiddyDbContext db, string degreeType)
        {
            degreeType = degreeType.Trim();

            EducationalDegreeType educationalDegreeType = db.EducationalDegreeType
                .Where(s => s.IsDeleted == 0 && s.DegreeType == degreeType)
                .FirstOrDefault();

            if (educationalDegreeType == null)
            {
                educationalDegreeType = await GetOrAdd(db, "Not Specified");
            }
            return educationalDegreeType;
        }


        public static async Task<EducationalDegreeType> GetOrAdd(UpDiddyDbContext db, string degreeType)
        {
            degreeType = degreeType.Trim();

            EducationalDegreeType educationalDegreeType = db.EducationalDegreeType
                .Where(s => s.IsDeleted == 0 && s.DegreeType == degreeType)
                .FirstOrDefault();

            if (educationalDegreeType == null)
            {
                educationalDegreeType = CreateEducationalDegreeType(degreeType);
                db.EducationalDegreeType.Add(educationalDegreeType);
                db.SaveChanges();
            }
            return educationalDegreeType;
        }

        public static EducationalDegreeType GetEducationalDegreeTypeByDegreeType(UpDiddyDbContext db, string degreeType)
        {
            return db.EducationalDegreeType
                .Where(s => s.IsDeleted == 0 && s.DegreeType == degreeType)
                .FirstOrDefault();
        }
    }
}
