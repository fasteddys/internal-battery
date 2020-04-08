using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITagService
    {
        Task<TagListDto> GetTags(int limit, int offset, string sort, string order);
        Task<List<TagDto>> GetTagsByKeyword(string keyword);
        Task<TagDto> GetTag(Guid tagGuid);
        Task<Guid> CreateTag(TagDto tagDto);
        Task UpdateTag(Guid tagGuid, TagDto tagDto);
        Task DeleteTag(Guid tagGuid);
        Task<ProfileTagListDto> GetProfileTagsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task DeleteTagsFromProfileForRecruiter(Guid subscriberGuid, List<Guid> profileTagGuids);
        Task<List<Guid>> AddTagsToProfileForRecruiter(Guid subscriberGuid, List<Guid> tagGuids, Guid profileGuid);
    }
}