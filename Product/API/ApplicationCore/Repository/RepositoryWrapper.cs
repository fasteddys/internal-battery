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
        private IJobPostingRepository _jobPostingRepository;
        private ICompanyRepository _companyRepository;

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
    }
}
