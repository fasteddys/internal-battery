using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class InvalidEmailRepository : UpDiddyRepositoryBase<InvalidEmail>, IInvalidEmailRepository
    {
        public InvalidEmailRepository(UpDiddyDbContext dbContext)
            : base(dbContext)
        { }
    }
}
