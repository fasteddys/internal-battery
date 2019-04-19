using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;


namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EmploymentTypeFactory
    {
        static public EmploymentType GetEmploymentTypeByGuid(UpDiddyDbContext db, Guid EmploymentTypeGuid)
        {

            EmploymentType employmentType = db.EmploymentType
                .Where(c => c.IsDeleted == 0 && c.EmploymentTypeGuid == EmploymentTypeGuid)
                .FirstOrDefault();
            return employmentType;
        }
    }
}
