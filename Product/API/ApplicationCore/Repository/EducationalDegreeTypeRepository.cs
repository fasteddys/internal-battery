using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EducationalDegreeTypeRepository : UpDiddyRepositoryBase<EducationalDegreeType>, IEducationalDegreeTypeRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public EducationalDegreeTypeRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EducationalDegreeType>> GetAllEducationDegreeTypes()
        {
            return await (from e in _dbContext.EducationalDegreeType
                          where e.IsDeleted == 0
                          select e).ToListAsync();
        }

    }
}
