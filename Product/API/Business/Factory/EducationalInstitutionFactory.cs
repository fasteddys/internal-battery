using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Business.Factory
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

        static public EducationalInstitution GetOrAdd(UpDiddyDbContext db, string institutionName)
        {

            institutionName = institutionName.Trim();

            EducationalInstitution educationalInstitution = db.EducationalInstitution
                .Where(s => s.IsDeleted == 0 && s.Name == institutionName)
                .FirstOrDefault();

            if (educationalInstitution == null)
            {
                educationalInstitution = CreateEducationalInstitution(institutionName);
                db.EducationalInstitution.Add(educationalInstitution);
                db.SaveChanges();
            }
            return educationalInstitution;
        }
    }
}
