using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class HiringManagerCandidateSearchDto
    {
        public int PageSize { get; set; }
        public int PageNum { get; set; }

        public int NumPages { get; set; }

        public int CandidateCount { get; set; }

        public long TotalHits { get; set; }

        public double SearchTimeInMilliseconds { get; set; }

        public long SearchQueryTimeInTicks { get; set; }

        public long SearchMappingTimeInTicks { get; set; }

        public string SASToken { get; set; }

        public HiringManagerSearchFacetsDto Facets { get; set; } = new HiringManagerSearchFacetsDto();

        public ICollection<HiringManagerCandidateDto> Candidates { get; set; } = new List<HiringManagerCandidateDto>();

        #region Primary Search
        Guid CityGuid { get; set; }
        string Keyword { get; set; }
        int Radius { get; set; }

        #endregion


    }
}
