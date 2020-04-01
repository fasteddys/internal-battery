using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.Helpers.Job;
using Microsoft.Extensions.Configuration;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public CompanyService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration config)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = config;
        }

        public async Task<Guid> AddCompanyAsync(CompanyDto companyDto)
        {
            //TODO Address company logo URL in the future. Possibly a byte array represending the image which will be uploaded to azure blob storage 
            if (companyDto == null)
                throw new NullReferenceException("CompanyDto cannot be null");
            var company = _mapper.Map<Company>(companyDto);
            company.CompanyGuid = Guid.NewGuid();
            company.CreateDate = DateTime.Now;
            company.LogoUrl = string.Empty;
            BaseModelFactory.SetDefaultsForAddNew(company);
            await _repositoryWrapper.Company.AddCompany(company);
            return company.CompanyGuid;
        }

        public async Task EditCompanyAsync(CompanyDto companyDto)
        {
            if (companyDto == null)
                throw new NullReferenceException("CompanyDto cannot be null");
            var company = await _repositoryWrapper.Company.GetCompanyByCompanyGuid(companyDto.CompanyGuid);
            if (company == null)
                throw new NotFoundException("Company not found");
            if (company != null)
            {
                company.CompanyName = companyDto.CompanyName;
                company.IsHiringAgency = companyDto.IsHiringAgency;
                company.IsJobPoster = companyDto.IsJobPoster;
                company.ModifyDate = DateTime.Now;
                await _repositoryWrapper.Company.UpdateCompany(company);
            }
        }

        public async Task DeleteCompanyAsync(Guid companyGuid)
        {
            if (companyGuid == null || companyGuid == Guid.Empty)
                throw new NullReferenceException("CompanyGuid cannot be null");
            var company = await _repositoryWrapper.Company.GetByGuid(companyGuid);
            if (company == null)
                throw new NotFoundException("Company not found");
            if (company != null)
            {
                company.IsDeleted = 1;
                company.ModifyDate = DateTime.Now;
                await _repositoryWrapper.SaveAsync();
            }
        }

        public async Task<List<CompanyDto>> GetCompaniesAsync()
        {
            var queryableCompanies = _repositoryWrapper.Company.GetAllCompanies();
            //get only non deleted records
            return _mapper.Map<List<CompanyDto>>(await queryableCompanies.Where(c => c.IsDeleted == 0 && c.CompanyGuid != Guid.Empty).ToListAsync());
        }

        public async Task<CompanyListDto> GetCompanies(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var companies = await _repositoryWrapper.StoredProcedureRepository.GetCompanies(limit, offset, sort, order);
            if (companies == null)
                throw new NotFoundException("Companies not found");
            return _mapper.Map<CompanyListDto>(companies);
        }

        public async Task<CompanyDto> GetById(int id)
        {
            var entity = await _repositoryWrapper.Company.GetById(id);
            return _mapper.Map<CompanyDto>(entity);
        }

        public async Task<CompanyDto> GetByCompanyName(string companyName)
        {
            var context = _repositoryWrapper.Company.GetAll();
            return _mapper.Map<CompanyDto>(await context.Where(x => x.CompanyName == companyName).FirstOrDefaultAsync());
        }

        public async Task<CompanyDto> GetByCompanyGuid(Guid companyGuid)
        {
            if (companyGuid == null || companyGuid == Guid.Empty)
                throw new NullReferenceException("companyGuid cannot be null");
            var company = await _repositoryWrapper.Company.GetByGuid(companyGuid);
            if (company == null)
                throw new NotFoundException("company cannot be found");            
            return _mapper.Map<CompanyDto>(company);
        }
    }
}
