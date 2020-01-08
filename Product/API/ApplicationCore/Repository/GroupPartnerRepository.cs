using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class GroupPartnerRepository : UpDiddyRepositoryBase<GroupPartner>, IGroupPartnerRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public GroupPartnerRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GroupPartner> GetGroupPartnerByGroupIdPartnerIdAsync(int groupId, int partnerId)
        {
            return await (from gp in _dbContext.GroupPartner
                          where gp.GroupId == groupId && gp.PartnerId == partnerId
                          select gp).FirstOrDefaultAsync();
        }
    }
}
