using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using System;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobPostingService : IJobPostingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public JobPostingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }
        public async Task<List<KeyValuePair<int, int>>> GetJobCountPerProvinceAsync()
        {
            var query = await _repositoryWrapper.JobPosting.GetAllAsync();
            List<KeyValuePair<int, int>> pairList = new List<KeyValuePair<int, int>>();
            List<string> provinceList = await query
            .Where(x => x.IsDeleted == 0)
            .Select(x => x.Province)
            .Distinct()
            .ToListAsync();
            foreach (var province in provinceList)
            {
                var count = query
                .Where(x => x.Province == province && x.IsDeleted == 0)
                .Count();
                var str = province.Trim().Replace(" ", "");
                Enums.ProvincePrefix e;
                if (Enum.TryParse(str.ToUpper(), out e))
                {
                    KeyValuePair<int, int> pair = new KeyValuePair<int, int>((int)e, count);
                    pairList.Add(pair);
                }
                else
                {
                    Enums.ProvinceName c;
                    if (Enum.TryParse(str.ToLower(), out c))
                    {
                        KeyValuePair<int, int> pair = new KeyValuePair<int, int>((int)c, count);
                        pairList.Add(pair);
                    }
                }
            }
            return pairList;
        }
    }
}