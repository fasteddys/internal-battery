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
