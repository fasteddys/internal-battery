using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface ITalentService
    {
        Task <List<TalentSearchOrderByDto>> GetTalentSearchOrderBy();
        Task<List<TalentSearchPartnersFilterDto>> GetTalentSearchPartnersFilter();

        Task<ProfileSearchResultDto> SearchTalent(int limit, int offset, string orderBy, string keyword, string location, string partner);

        Task<UpDiddyLib.Dto.SubscriberDto> TalentDetails(Guid subscriberGuid, Guid talentGuid,  bool isRecruiter);



    }
}
