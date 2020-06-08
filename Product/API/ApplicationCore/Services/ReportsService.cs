using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models.Reports;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public ReportsService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<UsersListDto> GetNewUsers()
        {
            var users = await _repositoryWrapper.StoredProcedureRepository.GetNewUsers();
            return _mapper.Map<UsersListDto>(users);
        }

        public async Task<List<UsersDetailDto>> GetAllUsersDetail()
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetAllUsersDetail();            
        }

        public async Task<List<UsersDetailDto>> GetAllHiringManagersDetail()
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetAllHiringManagersDetail();
        }

        public async Task<List<UsersDetailDto>> GetUsersByPartnerDetail(Guid partner, DateTime startDate, DateTime endDate)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetUsersByPartnerDetail(partner, startDate, endDate);
        }

        public async Task<List<PartnerUsers>> GetUsersByPartner(DateTime startDate, DateTime endDate)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetUsersByPartner(startDate, endDate);
        }
    }
}
