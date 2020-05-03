using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ITagRepository : IUpDiddyRepositoryBase<Tag>
    {
        Task<List<TagDto>> GetTags(int limit, int offset, string sort, string order);
        Task<List<ProfileTagDto>> GetProfileTagsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task DeleteTagsFromProfileForRecruiter(Guid subscriberGuid, List<Guid> profileTagGuids);
        Task<List<Guid>> AddTagsToProfileForRecruiter(Guid subscriberGuid, List<Guid> tagGuids, Guid profileGuid);
        Task<Guid> GetProfileGuidByProfileTagGuids(List<Guid> profileTagGuids);
    }
}