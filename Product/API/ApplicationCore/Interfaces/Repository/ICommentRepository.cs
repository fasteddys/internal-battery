using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICommentRepository : IUpDiddyRepositoryBase<ProfileComment>
    {
        Task<ProfileComment> GetCommentForRecruiter(Guid commentGuid, Guid subscriberGuid);
        Task<List<CommentDto>> GetProfileCommentsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<Guid> CreateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto);

        Task UpdateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto);
        Task DeleteCommentForRecruiter(Guid subscriberGuid, Guid commentGuid);

        Task<(Recruiter recruiter, List<Profile> profiles)> GetValidProfiles(Guid subscriberGuid, List<Guid> profileGuid);
    }
}