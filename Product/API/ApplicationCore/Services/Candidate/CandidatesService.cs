using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly ILogger _logger;

        public CandidatesService(
            ILogger<CandidatesService> logger
            )
        {
            _logger = logger;
        }

        // This empty service class will be used for stories:
        //   #2480 - Candidate 360: Personal Info
        //   #2481 - Candidate 360: Employment Preferences
        //   #2482 - Candidate 360: Role Preferences

        #region Personal Info

        #endregion Personal Info

        #region Employment Preferences

        #endregion Employment Preferences

        #region Role Preferences

        #endregion Role Preferences
    }
}
