using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Services.B2B
{
    public class InterviewRequestService : IInterviewRequestService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISendGridService _sendGridService;

        public InterviewRequestService(IRepositoryWrapper repositoryWrapper, ISendGridService sendGridService)
        {
            _repositoryWrapper = repositoryWrapper;
            _sendGridService = sendGridService;
        }

        public async Task SubmitInterviewRequest(Guid hiringManagerGuid, Guid profileGuid)
        {
            var profile = await _repositoryWrapper.ProfileRepository.GetByGuid(profileGuid);
            if (profile == null) { throw new NotFoundException("profile was not found"); }

            var hiringManager = await _repositoryWrapper.HiringManagerRepository.GetByGuid(hiringManagerGuid);
            if (hiringManager == null) { throw new NotFoundException("hiring manager not found."); }

            // TODO:  Hangfire?

            // Stub:  Send email to candidate
            // Stub:  Send email to recruiter

            var interviewRequest = new InterviewRequest
            {
                InterviewRequestGuid = Guid.NewGuid(),
                CreateGuid = Guid.NewGuid(),
                HiringManager = hiringManager,
                Profile = profile,
                DateRequested = DateTime.UtcNow
            };

            await _repositoryWrapper.InterviewRequestRepository.Create(interviewRequest);
            await _repositoryWrapper.InterviewRequestRepository.SaveAsync();
        }
    }
}
