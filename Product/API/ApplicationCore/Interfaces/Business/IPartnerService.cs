using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IPartnerService
    {
        Task<PartnerDto> GetPartner(Guid partnerGuid);
        Task<PartnerListDto> GetPartners(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdatePartner(Guid partnerGuid, PartnerDto partnerDto);
        Task CreatePartner(PartnerDto partnerDto);
        Task DeletePartner(Guid partnerGuid);
    }
}
