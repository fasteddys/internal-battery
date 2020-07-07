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

        public async Task<List<EducationalDegreeType>> GetAllDefinedEducationDegreeTypes(int limit, int offset, string sort, string order)
        {
            //filter only the predefined EducationDegreeTypes
            var educationalDegreeTypesQuery = _dbContext.EducationalDegreeType
                .Where(edt => edt.IsDeleted == 0 && edt.IsVerified.HasValue && edt.IsVerified.Value)
                .Include(edt => edt.EducationalDegreeTypeCategory)
                .OrderBy(edt => edt.EducationalDegreeTypeCategory.Sequence)
                .Skip(limit * offset)
                .Take(limit);

            //sorting            
            if (order.ToLower() == "descending")
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderByDescending(s => s.ModifyDate);
                        break;
                    case "createdate":
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderByDescending(s => s.CreateDate);
                        break;
                    case "sequence":
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderByDescending(s => s.Sequence);
                        break;
                    default:
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderByDescending(s => s.Sequence);
                        break;
                }
            }
            else
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderBy(s => s.ModifyDate);
                        break;
                    case "createdate":
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderBy(s => s.CreateDate);
                        break;
                    case "sequence":
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderBy(s => s.Sequence);
                        break;
                    default:
                        educationalDegreeTypesQuery = educationalDegreeTypesQuery.OrderBy(s => s.Sequence);
                        break;
                }
            }


            return await educationalDegreeTypesQuery.ToListAsync();
        }


    }
}
