using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ProfileSearchResultDto
    {
        public string RequestId { get; set; }
        public string ClientEventId { get; set; }
        public int PageSize { get; set; }
        public int PageNum { get; set; }
        public int JobCount { get; set; }
        public long TotalHits { get; set; }
        public int NumPages { get; set; }

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

        public List<ProfileViewDto> Profiles { get; set; } = new List<ProfileViewDto>();

    }
}
