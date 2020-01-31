using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITalentFavoriteService
    {
        Task AddToFavorite(Guid subscriberGuid, Guid talentGuid);
        Task RemoveFromFavorite(Guid subscriberGuid, Guid talentGuid);
        Task<TalentFavoriteListDto> GetFavoriteTalent(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
    }
}
