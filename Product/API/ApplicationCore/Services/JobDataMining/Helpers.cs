using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public static class Helpers
    {
        public class EqualityComparerByUri : IEqualityComparer<JobPage>
        {
            public bool Equals(JobPage x, JobPage y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.Uri == y.Uri;
            }

            public int GetHashCode(JobPage jobPage)
            {
                if (Object.ReferenceEquals(jobPage, null))
                    return 0;
                return jobPage.Uri == null ? 0 : jobPage.Uri.GetHashCode();
            }
        }

        public class CompareByUri : IComparer<JobPage>
        {
            public int Compare(JobPage x, JobPage y)
            {
                return string.Compare(x.Uri.ToString(), y.Uri.ToString());
            }
        }
    }
}
