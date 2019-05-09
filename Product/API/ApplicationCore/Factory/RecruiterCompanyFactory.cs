using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class RecruiterCompanyFactory
    {
        public static List<RecruiterCompany> GetRecruiterCompanyById(UpDiddyDbContext db, int subscriberId)
        {
            return db.RecruiterCompany
               .Include(s => s.Company)
               .Include(s => s.Recruiter.Subscriber)
               .Where(rc => rc.IsDeleted == 0 && rc.Recruiter.SubscriberId == subscriberId)
               .ToList();
        }

        public static RecruiterCompany CreateRecruiterCompany(UpDiddyDbContext db, int recruiterId, int companyId, bool isStaff)
        {

            RecruiterCompany recruiterCompany = new RecruiterCompany();
            recruiterCompany.CreateDate = DateTime.UtcNow;
            recruiterCompany.CreateGuid = Guid.Empty;
            recruiterCompany.RecruiterId = recruiterId;
            recruiterCompany.CompanyId = companyId;
            recruiterCompany.IsDeleted = 0;
            recruiterCompany.IsStaff = (isStaff) ? 1 : 0;
            recruiterCompany.RecruiterCompanyGuid = Guid.NewGuid();
            return recruiterCompany;
        }

        public static RecruiterCompany GetOrAdd(UpDiddyDbContext db, int recruiterId, int companyId, bool isStaff)
        {
            RecruiterCompany recruiterCompany = db.RecruiterCompany
                .Include(rc => rc.Recruiter)
                .Include(rc => rc.Company)
                .Where(rc => rc.IsDeleted == 0 && rc.Recruiter.RecruiterId == recruiterId && rc.Company.CompanyId == companyId)
                .FirstOrDefault();

            if (recruiterCompany == null)
            {
                recruiterCompany = CreateRecruiterCompany(db, recruiterId, companyId, isStaff);
                db.RecruiterCompany.Add(recruiterCompany);
                db.SaveChanges();
            }
            return recruiterCompany;
        }
    }
}
