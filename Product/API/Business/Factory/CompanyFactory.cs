using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Business.Factory
{
    public class CompanyFactory
    {

        static public Company CreateCompany(string companyName)
        {
            Company rVal = new Company();
            rVal.CompanyName = companyName;
            rVal.CreateDate = DateTime.Now;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyDate = DateTime.Now;
            rVal.ModifyGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public Company GetOrAdd(UpDiddyDbContext db, string companyName)
        {            
            companyName = companyName.Trim();
            Company company = db.Company
                .Where(c => c.IsDeleted == 0 && c.CompanyName == companyName)
                .FirstOrDefault();

            if (company == null)
            {
                company = CreateCompany(companyName);
                db.Company.Add(company);
                db.SaveChanges();
            }
            return company;
        }

    }
}
