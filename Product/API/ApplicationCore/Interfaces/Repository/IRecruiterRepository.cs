using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IRecruiterRepository :IUpDiddyRepositoryBase<Recruiter>
    {
        Task<IQueryable<Recruiter>> GetAllRecruiters();
        Task<Recruiter> GetRecruiterBySubscriberId(int subscriberId);
        Task<Recruiter> GetRecruiterByRecruiterGuid(Guid recruiterGuid);
        Task AddRecruiter(Recruiter recruiter);
        Task UpdateRecruiter(Recruiter recruiter);
    }
}
