using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class EducationalInstitution
    {

        public EducationalInstitution()
        {

        }
        public EducationalInstitution(string institutionName)
        {
            this.Name = institutionName;
            this.CreateDate = DateTime.Now;
            this.CreateGuid = Guid.NewGuid();
            this.ModifyDate = DateTime.Now;
            this.ModifyGuid = Guid.NewGuid();
            this.IsDeleted = 0;
        }

        static public EducationalInstitution GetOrAdd(UpDiddyDbContext db, string institutionName)
        {

            institutionName = institutionName.Trim();

            EducationalInstitution educationalInstitution = db.EducationalInstitution
                .Where(s => s.IsDeleted == 0 && s.Name == institutionName)
                .FirstOrDefault();

            if (educationalInstitution == null)
            {
                educationalInstitution = new EducationalInstitution(institutionName);
                db.EducationalInstitution.Add(educationalInstitution);
                db.SaveChanges();
            }
            return educationalInstitution;
        }

    }
}
