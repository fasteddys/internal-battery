using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IJobDataMining
    {
        Task<List<JobPage>> DiscoverJobPages(List<JobPage> existingJobPages);
        JobPostingDto ProcessJobPage(JobPage jobPage);
    }
}
