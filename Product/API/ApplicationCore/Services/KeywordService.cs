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

        
        public async Task<List<SearchTermDto>> GetKeywordSearchTerms(string value) 
        {

            if (string.IsNullOrEmpty(value))
                throw new NullReferenceException("Value cannot be null or empty");
            value = value.ToLower();
            // first try for a direct cache hit 
            string searchCacheKey = $"GetKeywordSearchTerms" + value;
            List<SearchTermDto> rval = (List<SearchTermDto>)_memoryCacheService.GetCacheValue(searchCacheKey);
            if (rval == null)
            {
                // get the results from the master search list 
                string cacheKey = $"GetKeywordSearchTerms";
                // get master search list from cache 
                List<SearchTermDto> searchList = (List<SearchTermDto>)_memoryCacheService.GetCacheValue(cacheKey);
                if (searchList == null)
                {
                    // get master search list from database 
                    searchList = await _repositoryWrapper.StoredProcedureRepository.GetKeywordSearchTermsAsync();
                    _memoryCacheService.SetCacheValue(cacheKey, searchList);
                }

                // get the results from the master search list 
                rval = searchList?.Where(v => v.Value.Contains(value))?.ToList();
                // cache the search results 
                _memoryCacheService.SetCacheValue(searchCacheKey, rval);

            }
            return rval;
        }


        public async Task<List<SearchTermDto>> GetLocationSearchTerms(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new NullReferenceException("Value cannot be null or empty");

            value = value.ToLower();
            // first try for a direct cache hit 
            string searchCacheKey = $"GetLocationSearchTerms" + value;
            List<SearchTermDto> rval = (List<SearchTermDto>)_memoryCacheService.GetCacheValue(searchCacheKey);
            if (rval == null)
            {
                // get the results from the master search list 
                string cacheKey = $"GetLocationSearchTerms";
                // get master search list from cache 
                List<SearchTermDto> searchList = (List<SearchTermDto>)_memoryCacheService.GetCacheValue(cacheKey);
                if (searchList == null)
                {
                    // get master search list from database 
                    searchList = await _repositoryWrapper.StoredProcedureRepository.GetLocationSearchTermsAsync();
                    _memoryCacheService.SetCacheValue(cacheKey, searchList);
                }

                // get the results from the master search list 
                rval = searchList?.Where(v => v.Value.Contains(value))?.ToList();
                // cache the search results 
                _memoryCacheService.SetCacheValue(searchCacheKey, rval);

            }
            return rval;
        }


    }
}