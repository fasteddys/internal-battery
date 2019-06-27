using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ProfileQuery
    {
        public string Query { get; set; }
        public List<LocationFilter> LocationFilters { get; set; }
        public List<JobTitleFilter> JobTitleFilters { get; set; }
        public List<EmployerFilter> EmployerFilters { get; set; }
        public List<EducationFilter> EducationFilters { get; set; }
        public List<SkillFilter> SkillFilters { get; set; }

        public List<WorkExperienceFilter> WorkExperienceFilters { get; set; }

        public List<TimeFilter> TimeFilters { get; set; }

        public bool HirableFilter { get; set; }

        public List<ApplicationDateFilter> ApplicationDateFilters { get; set; }

        public List<ApplicationOutcomeNotesFilter> ApplicationOutcomeNotesFilters { get; set; }

        public List<ApplicationJobFilter> ApplicationJobFilters { get; set; }

        public string CustomAttributeFilter { get; set; }

        public List<CandidateAvailabilityFilter> CandidateAvailabilityFilters { get; set; }

 







    }
}
