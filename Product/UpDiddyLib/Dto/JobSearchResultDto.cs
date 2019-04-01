using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobSearchResultDto
    {       
        public int PageNum { get; set; }
        public int JobCount { get; set; } 

        public List<JobViewDto> Jobs { get; set; } = new List<JobViewDto>();

        public List<JobQueryFacetDto> Facets { get; set; } = new List<JobQueryFacetDto>();
    }
}
