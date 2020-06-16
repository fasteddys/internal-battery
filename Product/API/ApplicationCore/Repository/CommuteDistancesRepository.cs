using Microsoft.EntityFrameworkCore;
using System;
using UpDiddyApi.ApplicationCore.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CommuteDistancesRepository : UpDiddyRepositoryBase<CommuteDistance>, ICommuteDistancesRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public CommuteDistancesRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CommuteDistance>> GetCommuteDistances(int limit, int offset, string sort, string order)
        {
            var commuteDistances = _dbContext.CommuteDistance
                .Where(cd => cd.IsDeleted == 0)
                .Skip(limit * offset)
                .Take(limit);

            //sorting            
            if (order.ToLower() == "descending")
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        commuteDistances = commuteDistances.OrderByDescending(s => s.ModifyDate);
                        break;
                    case "createdate":
                        commuteDistances = commuteDistances.OrderByDescending(s => s.CreateDate);
                        break;
                    default:
                        commuteDistances = commuteDistances.OrderByDescending(s => s.ModifyDate);
                        break;
                }
            }
            else
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        commuteDistances = commuteDistances.OrderBy(s => s.ModifyDate);
                        break;
                    case "createdate":
                        commuteDistances = commuteDistances.OrderBy(s => s.CreateDate);
                        break;
                    default:
                        commuteDistances = commuteDistances.OrderBy(s => s.ModifyDate);
                        break;
                }
            }

            return await commuteDistances.ToListAsync();
        }

        public async Task<CommuteDistance> GetCommuteDistanceByGuid(Guid coummuteDistanceGuid)
        {
            var commuteDistances = await _dbContext.CommuteDistance
                .FirstOrDefaultAsync(cd => cd.IsDeleted == 0 && cd.CommuteDistanceGuid == coummuteDistanceGuid);

            return commuteDistances;
        }
    }
}
