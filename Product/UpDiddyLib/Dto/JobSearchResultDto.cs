using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobSearchResultDto
    {       
        public int PageNum { get; set; }
        public int JobCount { get; set; }
        public int TotalHits { get; set; }

        /// <summary>
        /// 
        /// Total time for the job search 
        /// </summary>
        public double SearchTimeInMilliseconds { get; set; }
        /// <summary>
        /// Time for cloud talent to return search results
        /// </summary>
        public long SearchQueryTimeInTicks { get; set; }
        /// <summary>
        /// Time to map cloud talent search results to cc jobview 
        /// </summary>
        public long SearchMappingTimeInTicks { get; set; }

        public List<JobViewDto> Jobs { get; set; } = new List<JobViewDto>();

        public List<JobQueryFacetDto> Facets { get; set; } = new List<JobQueryFacetDto>();
    }
}
