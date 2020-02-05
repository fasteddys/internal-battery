using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.Reports;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IReportsService
    {
        Task<UsersListDto> GetNewUsers();
        Task<List<UsersDetailDto>> GetAllUsersDetail();
        Task<List<UsersDetailDto>> GetUsersByPartnerDetail(Guid partner, DateTime startDate, DateTime endDate);
        Task<List<PartnerUsers>> GetUsersByPartner(DateTime startDate, DateTime endDate);
    }
}
