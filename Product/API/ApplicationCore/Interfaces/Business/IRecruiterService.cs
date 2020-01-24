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
        Task EditRecruiterAsync(RecruiterInfoDto recruiterDto, Guid recruiter);
        Task DeleteRecruiterAsync(Guid subsceiberGuid, Guid recruiter);
        Task<RecruiterInfoDto> GetRecruiter(Guid recruiter);
    }
}
