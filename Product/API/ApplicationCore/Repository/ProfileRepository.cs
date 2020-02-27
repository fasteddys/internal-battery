using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ProfileRepository : UpDiddyRepositoryBase<Profile>, IProfileRepository
    {
        public ProfileRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
