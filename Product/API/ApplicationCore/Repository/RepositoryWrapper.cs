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
        private IJobApplicationRepository _jobApplicationRepository;
        private ICompanyRepository _companyRepository;
        private IJobSiteScrapeStatisticRepository _jobSiteScrapeStatisticRepository;
        private IRecruiterActionRepository _recruiterActionRepository;

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
                if(_jobSiteRepository == null)
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
                if(_jobPageRepository == null)
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

        public IJobApplicationRepository JobApplication
        {
            get
            {
                if(_jobApplicationRepository == null)
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
                if(_recruiterActionRepository == null)
                {
                    _recruiterActionRepository = new RecruiterActionRepository(_dbContext);
                }
                return _recruiterActionRepository;
            }
        }
    }
}
