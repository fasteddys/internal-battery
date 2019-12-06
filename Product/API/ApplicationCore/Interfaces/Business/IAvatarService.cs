using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IAvatarService
    {
        Task<string> GetAvatar(Guid subscriberGuid);
        Task UploadAvatar(Guid subscriberGuid, FileDto fileDto);
        Task RemoveAvatar(Guid subscriberGuid);
    }
}
