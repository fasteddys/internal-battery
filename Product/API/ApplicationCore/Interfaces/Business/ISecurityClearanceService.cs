using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISecurityClearanceService
    {
        Task<SecurityClearanceDto> GetSecurityClearance(Guid securityClearanceGuid);
        Task<SecurityClearanceListDto> GetSecurityClearances(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateSecurityClearance(Guid securityClearanceGuid, SecurityClearanceDto securityClearanceDto);
        Task CreateSecurityClearance(SecurityClearanceDto securityClearanceDto);
        Task DeleteSecurityClearance(Guid securityClearanceGuid);
    }
}
