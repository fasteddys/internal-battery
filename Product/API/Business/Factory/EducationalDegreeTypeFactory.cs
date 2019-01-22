using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Business.Factory
{
    public class EducationalDegreeTypeFactory
    {
        public static EducationalDegreeType CreateEducationalDegreeType(string degreeType)
        {
            EducationalDegreeType rVal = new EducationalDegreeType();
            rVal.DegreeType = degreeType;
            rVal.CreateDate = DateTime.Now;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyDate = DateTime.Now;
            rVal.ModifyGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public EducationalDegreeType GetOrAdd(UpDiddyDbContext db, string degreeType)
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
