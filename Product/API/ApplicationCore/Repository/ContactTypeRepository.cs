using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ContactTypeRepository : UpDiddyRepositoryBase<ContactType>, IContactTypeRepository
    {
        public ContactTypeRepository(UpDiddyDbContext dbContext)
            : base(dbContext)
        { }
    }
}
