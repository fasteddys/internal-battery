using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly UpDiddyDbContext _dbContext;
        private ICountryRepository _countryRepository;
        private IStateRepository _stateRepository;
        private IJobSiteRepository _jobSiteRepository;
        private IJobPageRepository _jobPageRepository;
        private IJobPostingRepository _jobPostingRepository;
        private IJobPostingFavoriteRepository _jobPostingFavoriteRepository;
        private IJobApplicationRepository _jobApplicationRepository;
        private ICompanyRepository _companyRepository;
        private IJobSiteScrapeStatisticRepository _jobSiteScrapeStatisticRepository;
        private IRecruiterActionRepository _recruiterActionRepository;
        private ISubscriberRepository _subscriberRepository;
        private IZeroBounceRepository _zeroBounceRepository;
        private IPartnerContactLeadStatusRepository _partnerContactLeadStatusRepository;
        private IJobCategoryRepository _jobCategoryRepository;
        private IJobReferralRepository _jobReferralRepository;
        private ISubscriberNotesRepository _subscriberNotesRepository;
        private IRecruiterRepository _recruiterRepository;
        private IJobPostingAlertRepository _jobPostingAlertRepository;
        private IResumeParseRepository _resumeParseRepository;
        private IResumeParseResultRepository _resumeParseResultRepository;

        public RepositoryWrapper(UpDiddyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

  

        public ICountryRepository Country
        {
            get
            {
                if (_countryRepository == null)
                {
                    _countryRepository = new CountryRepository(_dbContext, State);
                }
                return _countryRepository;
            }
        }

        public IStateRepository State
        {
            get
            {
                if (_stateRepository == null)
                {
                    _stateRepository = new StateRepository(_dbContext);
                }
                return _stateRepository;
            }
        }

        public IJobSiteRepository JobSite
        {
            get
            {
                if (_jobSiteRepository == null)
                {
                    _jobSiteRepository = new JobSiteRepository(_dbContext);
                }
                return _jobSiteRepository;
            }
        }

        public IJobPageRepository JobPage
        {
            get
            {
                if (_jobPageRepository == null)
                {
                    _jobPageRepository = new JobPageRepository(_dbContext);
                }
                return _jobPageRepository;
            }
        }

        public IJobSiteScrapeStatisticRepository JobSiteScrapeStatistic
        {
            get
            {
                if (_jobSiteScrapeStatisticRepository == null)
                {
                    _jobSiteScrapeStatisticRepository = new JobSiteScrapeStatisticRepository(_dbContext);
                }
                return _jobSiteScrapeStatisticRepository;
            }
        }

        public IJobPostingRepository JobPosting
        {
            get
            {
                if (_jobPostingRepository == null)
                {
                    _jobPostingRepository = new JobPostingRepository(_dbContext);
                }
                return _jobPostingRepository;
            }
        }

        public IJobPostingFavoriteRepository JobPostingFavorite
        {
            get
            {
                if (_jobPostingFavoriteRepository == null)
                {
                    _jobPostingFavoriteRepository = new JobPostingFavoriteRepository(_dbContext);
                }
                return _jobPostingFavoriteRepository;
            }
        }

        public IJobApplicationRepository JobApplication
        {
            get
            {
                if (_jobApplicationRepository == null)
                {
                    _jobApplicationRepository = new JobApplicationRepository(_dbContext);
                }
                return _jobApplicationRepository;
            }
        }

        public ICompanyRepository Company
        {
            get
            {
                if (_companyRepository == null)
                {
                    _companyRepository = new CompanyRepository(_dbContext);
                }
                return _companyRepository;
            }
        }


        public IRecruiterActionRepository RecruiterActionRepository
        {
            get
            {
                if (_recruiterActionRepository == null)
                {
                    _recruiterActionRepository = new RecruiterActionRepository(_dbContext);
                }
                return _recruiterActionRepository;
            }
        }

        public ISubscriberRepository Subscriber
        {
            get
            {
                if (_subscriberRepository == null)
                {
                    _subscriberRepository = new SubscriberRepository(_dbContext);
                }
                return _subscriberRepository;
            }
        }

        public IZeroBounceRepository ZeroBounceRepository
        {
            get
            {
                if (_zeroBounceRepository == null)
                {
                    _zeroBounceRepository = new ZeroBounceRepository(_dbContext);
                }
                return _zeroBounceRepository;
            }
        }

        public IPartnerContactLeadStatusRepository PartnerContactLeadStatusRepository
        {
            get
            {
                if (_partnerContactLeadStatusRepository == null)
                {
                    _partnerContactLeadStatusRepository = new PartnerContactLeadStatusRepository(_dbContext);
                }
                return _partnerContactLeadStatusRepository;
            }
        }

        public IJobCategoryRepository JobCategoryRepository
        {
            get
            {
                if (_jobCategoryRepository == null)
                {
                    _jobCategoryRepository = new JobCategoryRepository(_dbContext);
                }
                return _jobCategoryRepository;
            }
        }


        public ISubscriberRepository SubscriberRepository
        {
            get
            {
                if (_subscriberRepository == null)
                {
                    _subscriberRepository = new SubscriberRepository(_dbContext);
                }
                return _subscriberRepository;
            }
        }

        public IJobReferralRepository JobReferralRepository
        {
            get
            {
                if (_jobReferralRepository == null)
                {
                    _jobReferralRepository = new JobReferralRepository(_dbContext);
                }
                return _jobReferralRepository;
            }
        }

        public IJobPostingAlertRepository JobPostingAlertRepository
        {
            get
            {
                if (_jobPostingAlertRepository == null)
                {
                    _jobPostingAlertRepository = new JobPostingAlertRepository(_dbContext);
                }
                return _jobPostingAlertRepository;
            }
        }

        public ISubscriberNotesRepository SubscriberNotesRepository
        {
            get
            {
                if (_subscriberNotesRepository == null)
                {
                    _subscriberNotesRepository = new SubscriberNotesRepository(_dbContext);
                }
                return _subscriberNotesRepository;
            }
        }

        public IRecruiterRepository RecruiterRepository
        {
            get
            {
                if (_recruiterRepository == null)
                {
                    _recruiterRepository = new RecruiterRepository(_dbContext);
                }
                return _recruiterRepository;
            }
        }


        public IResumeParseRepository ResumeParseRepository
        {
            get
            {
                if (_resumeParseRepository == null)
                {
                    _resumeParseRepository = new ResumeParseRepository(_dbContext);
                }
                return _resumeParseRepository;
            }
        }

        public IResumeParseResultRepository ResumeParseResultRepository
        {
            get
            {
                if (_resumeParseResultRepository == null)
                {
                    _resumeParseResultRepository = new ResumeParseResultRespository(_dbContext);
                }
                return _resumeParseResultRepository;
            }
        }
    }
}
