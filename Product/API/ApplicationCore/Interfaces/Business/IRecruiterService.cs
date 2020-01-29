using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IRecruiterService
    {
        Task<List<RecruiterDto>> GetRecruitersAsync();
        Task<string> AddRecruiterAsync(RecruiterDto recruiterDto);
        Task EditRecruiterAsync(RecruiterDto recruiterDto);
        Task DeleteRecruiterAsync(RecruiterDto recruiterDto);
        Task<RecruiterInfoListDto> GetRecruiters(int limit, int offset, string sort, string order);
        Task<bool> AddRecruiterAsync(RecruiterInfoDto recruiterDto);
        Task EditRecruiterAsync(RecruiterInfoDto recruiterDto, Guid Recruiter);
        Task DeleteRecruiterAsync(Guid subsceiberGuid, Guid recruiterDto);

        Task<RecruiterInfoDto> GetRecruiterAsync(Guid RecruiterGuid);

        Task<RecruiterInfoDto> GetRecruiterBySubscriberAsync(Guid SubscriberGuid);

        Task<RecruiterSearchResultDto> SearchRecruitersAsync(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", string companyName = "");

    }
}
