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
    public class RecruiterService : IRecruiterService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public RecruiterService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }
        public async Task<List<RecruiterDto>> GetRecruitersAsync()
        {
            var queryableRecruiters = await _repositoryWrapper.RecruiterRepository.GetAllRecruiters();
            //get only non deleted records
            return _mapper.Map<List<RecruiterDto>>(await queryableRecruiters.Where(c => c.IsDeleted == 0 && c.RecruiterGuid != Guid.Empty).ToListAsync());
        }
    }
}
