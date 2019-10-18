using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IPartnerContactRepository : IUpDiddyRepositoryBase<PartnerContact>
    {
        Task<IList<Partner>> GetPartnersAssociatedWithSubscriber(int SubscriberId);

    }
}

