using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CompensationTypeRepository : UpDiddyRepositoryBase<CompensationType>, ICompensationTypeRepository
    {
         private readonly UpDiddyDbContext _dbContext;
        public CompensationTypeRepository(UpDiddyDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
         }

        public async Task<List<CompensationType>> GetAllCompensationTypes()
        {
            return await (from e in _dbContext.CompensationType
                          where e.IsDeleted == 0
                          select e ).ToListAsync();
        }
    }
}
