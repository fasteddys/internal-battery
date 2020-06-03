using System.Threading.Tasks;
using UpDiddyApi.Models.G2.CrossChq;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.G2
{
    public interface ICrossChqWebClient
    {
        Task<string> PostReferenceRequestAsync(ReferenceRequest request);

        Task<ReferenceResponse> GetReferenceRequestAsync(string referenceId);
    }
}
