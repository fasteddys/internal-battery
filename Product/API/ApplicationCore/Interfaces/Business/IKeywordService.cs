using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IKeywordService
    {
        Task<List<SearchTermDto>> GetKeywordSearchTerms(string keyword);
    }
}
