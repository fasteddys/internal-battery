using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Company
    {

        #region Factory Methods

        public Company(string companyName)
        {
            this.CompanyName = companyName;
            this.CreateDate = DateTime.Now;
            this.CreateGuid = Guid.NewGuid();
            this.ModifyDate = DateTime.Now;
            this.ModifyGuid = Guid.NewGuid();
            this.IsDeleted = 0;
        }

        static public Company GetOrAdd(UpDiddyDbContext db, string companyName)
        {
            companyName = companyName.Trim();

            Company company = db.Company
                .Where(c => c.IsDeleted == 0 && c.CompanyName == companyName)
                .FirstOrDefault();

            if (company == null)
            {
                company = new Company(companyName);
                db.Company.Add(company);
                db.SaveChanges();
            }
            return company;
        }

        #endregion


    }
}
