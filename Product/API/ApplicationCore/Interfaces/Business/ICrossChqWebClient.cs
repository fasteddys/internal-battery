using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICrossChqWebClient
    {
        Task<string> PostReferenceRequestAsync(ReferenceRequestDto request);

        Task<ReferenceResponseDto> GetReferenceRequestAsync(string referenceId);
    }
}
