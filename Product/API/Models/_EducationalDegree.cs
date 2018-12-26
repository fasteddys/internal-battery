using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class EducationalDegree
    {
        public EducationalDegree()
        {

        }
        public EducationalDegree(string degree)
        {
            this.Degree = degree;
            this.CreateDate = DateTime.Now;
            this.CreateGuid = Guid.NewGuid();
            this.ModifyDate = DateTime.Now;
            this.ModifyGuid = Guid.NewGuid();
            this.IsDeleted = 0;
        }

        static public EducationalDegree GetOrAdd(UpDiddyDbContext db, string degree)
        {
            degree = degree.Trim();

            EducationalDegree educationalDegree = db.EducationalDegree
                .Where(s => s.IsDeleted == 0 && s.Degree == degree)
                .FirstOrDefault();

            if (educationalDegree == null)
            {
                educationalDegree = new EducationalDegree(degree);
                db.EducationalDegree.Add(educationalDegree);
                db.SaveChanges();
            }
            return educationalDegree;
        }


    }
}
