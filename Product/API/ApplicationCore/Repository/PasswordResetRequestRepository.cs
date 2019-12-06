using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class PasswordResetRequestRepository : UpDiddyRepositoryBase<PasswordResetRequest>, IPasswordResetRequestRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public PasswordResetRequestRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
