using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class TrainingTypesRepository : UpDiddyRepositoryBase<TrainingType>, ITrainingTypesRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public TrainingTypesRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<TrainingType>> GetAllTrainingTypes(int limit, int offset, string sort, string order)
        {
            var trainingTypesQuery = _dbContext.TrainingType
                .Where(edt => edt.IsDeleted == 0)
                .Skip(limit * offset)
                .Take(limit);

            //sorting            
            if (order.ToLower() == "descending")
            {
                switch (sort.ToLower())
                {
                    case "createdate":
                        trainingTypesQuery = trainingTypesQuery.OrderByDescending(s => s.CreateDate);
                        break;
                    case "sequence":
                        trainingTypesQuery = trainingTypesQuery.OrderByDescending(s => s.Sequence);
                        break;
                    default:
                        trainingTypesQuery = trainingTypesQuery.OrderByDescending(s => s.Sequence);
                        break;
                }
            }
            else
            {
                switch (sort.ToLower())
                {
                    case "createdate":
                        trainingTypesQuery = trainingTypesQuery.OrderBy(s => s.CreateDate);
                        break;
                    case "sequence":
                        trainingTypesQuery = trainingTypesQuery.OrderBy(s => s.Sequence);
                        break;
                    default:
                        trainingTypesQuery = trainingTypesQuery.OrderBy(s => s.Sequence);
                        break;
                }
            }


            return await trainingTypesQuery.ToListAsync();
        }
    }
}
