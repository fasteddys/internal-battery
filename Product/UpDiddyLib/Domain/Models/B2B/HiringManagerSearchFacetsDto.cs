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
        public ICollection<SalaryFacetResultDto> Salary { get; set; } = new List<SalaryFacetResultDto>();
        public ICollection<FacetResultWithQueryDto> Resume { get; set; } = new List<FacetResultWithQueryDto>();
        public ICollection<FacetResultWithQueryDto> Video { get; set; } = new List<FacetResultWithQueryDto>();
    }

    public class FacetResultWithQueryDto
    {
        public long? Count { get; set; }
        public string Value { get; set; }
        public string Query { get; set; }
        public bool IsSelected { get; set; }

    }

    public class SalaryFacetResultDto
    {
        public long? Count { get; set; }
        public int? Value { get; set; }
        public bool IsUbSelected { get; set; }
        public bool IsLbSelected { get; set; }
    }
}
