using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public CompanyService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }
        public async Task<List<CompanyDto>> GetCompaniesAsync()
        {
            var queryableCompanies = await _repositoryWrapper.Company.GetAllCompanies();
            return _mapper.Map<List<CompanyDto>>(await queryableCompanies.ToListAsync());
        }
    }
}
