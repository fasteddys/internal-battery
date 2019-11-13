using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces;

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

        public async Task<List<SearchTermDto>> GetKeywordSearchTerms(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new NullReferenceException("Value cannot be null or empty");
            string cacheKey = $"GetKeywordSearchTerms";
            IList<SearchTermDto> rval = (IList<SearchTermDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (rval == null)
            {
                rval = await _repositoryWrapper.StoredProcedureRepository.GetKeywordSearchTermsAsync();
                _memoryCacheService.SetCacheValue(cacheKey, rval);
            }
            return rval?.Where(v => v.Value.Contains(value))?.ToList();
        }
    }
}
