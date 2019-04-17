using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobQueryFacetDto
    {
        public string Name { get; set; }
        public List<JobQueryFacetItemDto> Facets { get; set; }  = new List<JobQueryFacetItemDto>();
    }
}
