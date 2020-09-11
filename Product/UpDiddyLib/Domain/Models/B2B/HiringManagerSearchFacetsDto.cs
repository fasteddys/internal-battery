using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class HiringManagerSearchFacetsDto
    {

        public ICollection<FacetResultWithQueryDto> WorkPreferences { get; set; } = new List<FacetResultWithQueryDto>();
        public ICollection<FacetResultWithQueryDto> Skills { get; set; } = new List<FacetResultWithQueryDto>();
        public ICollection<FacetResultWithQueryDto> Certifications { get; set; } = new List<FacetResultWithQueryDto>();
        public ICollection<FacetResultWithQueryDto> Personality { get; set; } = new List<FacetResultWithQueryDto>();
        public ICollection<FacetResultWithQueryDto> RolePreferences { get; set; } = new List<FacetResultWithQueryDto>();
        public ICollection<FacetResultDto> Salary { get; set; } = new List<FacetResultDto>();
        public ICollection<FacetResultDto> Resume { get; set; } = new List<FacetResultDto>();
        public ICollection<FacetResultDto> Video { get; set; } = new List<FacetResultDto>();
    }

    public class FacetResultWithQueryDto
    {
        public long? Count { get; set; }
        public string Value { get; set; }
        public string Query { get; set; }

    }

    public class FacetResultDto
    {
        public long? Count { get; set; }
        public string Value { get; set; }
    }
}
