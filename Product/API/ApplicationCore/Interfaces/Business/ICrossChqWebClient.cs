using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICrossChqWebClient
    {
        Task<string> PostReferenceRequestAsync(ReferenceRequest request);

        Task<ReferenceResponse> GetReferenceRequestAsync(string referenceId);
    }
}
