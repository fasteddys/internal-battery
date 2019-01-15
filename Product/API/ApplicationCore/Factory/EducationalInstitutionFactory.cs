using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EducationalInstitutionFactory
    {
        static public EducationalInstitution CreateEducationalInstitution(string institutionName)
        {
            EducationalInstitution rVal = new EducationalInstitution();
            rVal.Name = institutionName;
            rVal.CreateDate = DateTime.Now;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyDate = DateTime.Now;
            rVal.ModifyGuid = Guid.NewGuid();
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
