using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EmploymentTypeRepository : UpDiddyRepositoryBase<EmploymentType>, IEmploymentTypeRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public EmploymentTypeRepository(UpDiddyDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
         }

        public async Task<List<EmploymentType>> GetAllEmploymentTypes()
        {
            return await (from e in _dbContext.EmploymentType
                          where e.IsDeleted == 0
                          select e ).ToListAsync();
        }
    }
}
