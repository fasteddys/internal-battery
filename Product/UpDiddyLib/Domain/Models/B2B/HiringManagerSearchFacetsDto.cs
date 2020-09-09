using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class HiringManagerSearchFacetsDto
    {

        public ICollection<FacetResultDto> WorkPreferences { get; set; }
        public ICollection<FacetResultDto> Skills { get; set; }
        public ICollection<FacetResultDto> Certifications { get; set; }
        public ICollection<FacetResultDto> Personality { get; set; }
        public ICollection<FacetResultDto> RolePreferences { get; set; }
        public ICollection<SalaryFacetResultDto> Salary { get; set; }
    }

    public class FacetResultDto
    {
        public int Count { get; set; }
        public string Value { get; set; }
        public string Query { get; set; }

    }

    public class SalaryFacetResultDto
    {
        public int Count { get; set; }
        public string Value { get; set; }
        public string Query { get; set; }

    }
}
