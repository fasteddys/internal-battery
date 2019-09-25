using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IPartnerRepository : IUpDiddyRepositoryBase<Partner>
    {
        Task<Partner> GetPartnerByName(string PartnerName);
        Task<Partner> GetPartnerByGuid(Guid partnerGuid);
        Task<Partner> GetOrCreatePartnerByName(string partnerName, PartnerType partnerType);

    }
}
