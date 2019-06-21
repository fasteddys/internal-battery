using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CompanyFactory
    {

        static public Company CreateCompany(string companyName)
        {
            Company rVal = new Company();
            rVal.CompanyName = companyName;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.CompanyGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public async Task<Company> GetOrAdd(UpDiddyDbContext db, string companyName)
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

        static public Company GetCompanyByGuid(UpDiddyDbContext db, Guid CompanyGuid)
        {
 
            Company company = db.Company
                .Where(c => c.IsDeleted == 0 && c.CompanyGuid == CompanyGuid)
                .FirstOrDefault();    
            return company;
        }

        static public Company GetCompanyByCompanyName(UpDiddyDbContext db, string  CompanyName)
        {

            Company company = db.Company
                .Where(c => c.IsDeleted == 0 && c.CompanyName == CompanyName)
                .FirstOrDefault();
            return company;
        }




    }
}
