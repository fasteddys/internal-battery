using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class CandidateSearchQueryDto
    {
        #region base search properties

        public int Limit { get; set; } = 10;
        public int Offset { get; set; } = 0;
        public string Sort { get; set; } = "ModifyDate";
        public string Order { get; set; } = "descending";
        public Guid CityGuid { get; set; }
        public string Keyword { get; set; } = "*";
        public int Radius { get; set; } = 0;

        #endregion

        #region facet filter properties

        public bool? HasVideoInterview { get; set; }
        public bool? IsResumeUploaded { get; set; }
        public int? SalaryLb { get; set; }
        public int? SalaryUb { get; set; }
        public ICollection<string> WorkPreference { get; set; }
        public ICollection<string> Skill { get; set; } 
        public ICollection<string> Certification { get; set; }
        public ICollection<string> Personality { get; set; }
        public ICollection<string> Role { get; set; }

        #endregion


    }
}
