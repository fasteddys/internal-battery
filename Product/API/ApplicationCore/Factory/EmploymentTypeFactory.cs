using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EmploymentTypeFactory
    {
        static public async Task<EmploymentType> GetEmploymentTypeByGuid(IRepositoryWrapper repositoryWrapper, Guid EmploymentTypeGuid)
        {
            EmploymentType employmentType = await repositoryWrapper.EmploymentTypeRepository.GetAllWithTracking()
                .Where(c => c.IsDeleted == 0 && c.EmploymentTypeGuid == EmploymentTypeGuid)
                .FirstOrDefaultAsync();
            return employmentType;
        }
    }
}
