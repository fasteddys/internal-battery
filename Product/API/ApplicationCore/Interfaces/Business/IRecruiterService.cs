using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IRecruiterService
    {
        Task<List<RecruiterDto>> GetRecruitersAsync();
        Task<string> AddRecruiterAsync(RecruiterDto recruiterDto);
        Task EditRecruiterAsync(RecruiterDto recruiterDto);
        Task DeleteRecruiterAsync(RecruiterDto recruiterDto);
    }
}
