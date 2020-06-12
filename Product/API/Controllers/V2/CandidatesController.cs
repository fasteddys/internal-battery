using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers.V2
{
    public class CandidatesController : BaseApiController
    {
        private readonly ICandidatesService _candidatesService;
        private readonly ILogger _logger;

        public CandidatesController(
            ICandidatesService candidatesService,
            ILogger<CandidatesController> logger
            )
        {
            _candidatesService = candidatesService;
            _logger = logger;
        }

        // This empty controller will be used for stories:
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
