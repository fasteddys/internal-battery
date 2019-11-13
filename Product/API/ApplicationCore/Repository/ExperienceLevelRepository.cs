using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ExperienceLevelRepository : UpDiddyRepositoryBase<ExperienceLevel>, IExperienceLevelRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ExperienceLevelRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ExperienceLevel>> GetAllExperienceLevels()
        {
            return await (from e in _dbContext.ExperienceLevel
                          where e.IsDeleted == 0
                          select e).ToListAsync();
        }
    }
}
