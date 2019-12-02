using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITalentFavoriteService
    {

        Task AddToFavorite(Guid subscriberGuid, Guid talentGuid);

        Task RemoveFromFavorite(Guid subscriberGuid, Guid talentGuid);

        Task<List<TalentFavoriteDto>> GetFavoriteTalent(Guid subscriberGuid, int limit, int offset, string sort, string order);
    }
}
