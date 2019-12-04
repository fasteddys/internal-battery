using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class KeywordService : IKeywordService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMemoryCacheService _memoryCacheService;
        public KeywordService(IRepositoryWrapper repositoryWrapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _memoryCacheService = memoryCacheService;
        }

        public async Task<List<SearchTermDto>> GetKeywordSearchTerms()
        {
            var keywords = await _repositoryWrapper.StoredProcedureRepository.GetKeywordSearchTermsAsync();
            if (keywords == null)
                throw new NotFoundException("Keywords not found");
            return keywords;
        }
    }
}