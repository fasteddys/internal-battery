using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ITalentFavoriteRepository : IUpDiddyRepositoryBase<TalentFavorite>
    {
        Task<TalentFavorite> GetBySubscriberGuidAndTalentGuid(Guid subscriberGuid, Guid talentGuid);
        Task<List<TalentFavorite>> GetTalentForSubscriber(Guid subscriberGuid, int limit = 30, int offset = 0, string sort = "CreateDate", string order = "descending");
    }
}
