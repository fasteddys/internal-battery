using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class HiringManagerCandidateSearchDto
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public int NumberOfPages { get; set; }

        public int CandidateCount { get; set; }

        public int TotalHits { get; set; }

        public decimal SearchTimeInMilliseconds { get; set; }

        public long SearchQueryTimeInTicks { get; set; }

        public long SearchMappingTimeInTicks { get; set; }

        public string SaasToken { get; set; }

        public HiringManagerSearchFacetsDto Facets { get; set; }

        public ICollection<HiringManagerCandidateDto> Candidates { get; set; }

        #region Primary Search
        Guid cityGuid { get; set; }
        string keyword { get; set; }
        int radius { get; set; }

        #endregion


    }
}
