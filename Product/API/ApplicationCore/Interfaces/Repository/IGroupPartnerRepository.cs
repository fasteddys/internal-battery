using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IGroupPartnerRepository : IUpDiddyRepositoryBase<GroupPartner>
    {
        Task<GroupPartner> GetGroupPartnerByGroupIdPartnerIdAsync(int groupId, int partnerId);
    }
}
