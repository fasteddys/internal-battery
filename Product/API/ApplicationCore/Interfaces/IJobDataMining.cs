﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IJobDataMining
    {
        List<JobPage> DiscoverJobPages(List<JobPage> existingJobPages);
        JobPosting ProcessJobPage(JobPage jobPage);
    }
}
