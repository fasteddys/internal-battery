using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.G2
{
   public interface ICommentService
    {
        Task<CommentDto> GetCommentForRecruiter(Guid commentGuid, Guid subscriberGuid);
        Task<Guid> CreateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto);
        Task UpdateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto);
        Task DeleteCommentForRecruiter(Guid subscriberGuid, Guid commentGuid);
        Task<CommentListDto> GetProfileCommentsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
    }
}
