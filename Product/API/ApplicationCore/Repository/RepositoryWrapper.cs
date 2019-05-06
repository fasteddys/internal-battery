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
    }
}
