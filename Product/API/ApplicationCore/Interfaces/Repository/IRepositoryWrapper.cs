using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IRepositoryWrapper
    {
        ICountryRepository Country { get; }
        IStateRepository State { get; }
        IJobSiteRepository JobSite { get; }
        IJobPageRepository JobPage { get; }
        IJobSiteScrapeStatisticRepository JobSiteScrapeStatistic { get; }
        IJobPostingRepository JobPosting { get; }
        IJobPostingFavoriteRepository JobPostingFavorite { get; }
        IJobApplicationRepository JobApplication { get; }
        ICompanyRepository Company { get; }
        IRecruiterActionRepository RecruiterActionRepository { get; }
        ISubscriberRepository Subscriber { get; }
        IZeroBounceRepository ZeroBounceRepository { get; }
        IPartnerContactLeadStatusRepository PartnerContactLeadStatusRepository { get; }
    }
}