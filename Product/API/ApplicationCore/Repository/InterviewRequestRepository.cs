using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class InterviewRequestRepository : UpDiddyRepositoryBase<InterviewRequest>, IInterviewRequestRepository
    {
        public InterviewRequestRepository(UpDiddyDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
