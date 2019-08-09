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

        public async Task AddCompanyAsync(CompanyDto companyDto)
        {
            var company=_mapper.Map<Company>(companyDto);

            //assign companyGuid
            company.CompanyGuid = Guid.NewGuid();
            BaseModelFactory.SetDefaultsForAddNew(company);

           await  _repositoryWrapper.Company.AddCompany(company);
        }

        public async Task EditCompanyAsync(CompanyDto companyDto)
        {
            var company = await _repositoryWrapper.Company.GetCompanyByCompanyGuid(companyDto.CompanyGuid);
            if(company!=null)
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
            var company = await _repositoryWrapper.Company.GetCompanyByCompanyGuid(companyGuid);
            if (company != null)
            {
                //set isDeleted to 1 to delete the record
                company.IsDeleted = 1;
                company.ModifyDate = DateTime.Now;

                await _repositoryWrapper.Company.UpdateCompany(company);
            }
        }

        public async Task<List<CompanyDto>> GetCompaniesAsync()
        {
            var queryableCompanies =  _repositoryWrapper.Company.GetAllCompanies();
            //get only non deleted records
            return _mapper.Map<List<CompanyDto>>(await queryableCompanies.Where(c=>c.IsDeleted==0 && c.CompanyGuid!=Guid.Empty).ToListAsync());
        }
    }
}
