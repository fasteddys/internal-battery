using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ProfileQuery
    {
        public string query { get; set; }
        public List<LocationFilter> locationFilters { get; set; }
        public List<JobTitleFilter> jobTitleFilters { get; set; }
        public List<EmployerFilter> employerFilters { get; set; }
        public List<EducationFilter> educationFilters { get; set; }
        public List<SkillFilter> skillFilters { get; set; }

        public List<WorkExperienceFilter> workExperienceFilters { get; set; }

        public List<TimeFilter> timeFilters { get; set; }

        public BoolValue hirableFilter { get; set; }

        public List<ApplicationDateFilter> applicationDateFilters { get; set; }

        public List<ApplicationOutcomeNotesFilter> applicationOutcomeNotesFilters { get; set; }

        public List<ApplicationJobFilter> applicationJobFilters { get; set; }

        public string customAttributeFilter { get; set; }

        public List<CandidateAvailabilityFilter> candidateAvailabilityFilters { get; set; }

 







    }
}
