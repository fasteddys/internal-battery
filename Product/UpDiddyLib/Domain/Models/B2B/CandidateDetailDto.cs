using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class CandidateDetailDto
    {
        public Guid ProfileGuid {  get; set; }
        public string FirstName { get; set; }
        public string JobTitle { get; set; }
        public string Location { get; set; }
        public List<string> Skills { get; set; } 
        public decimal? DesiredAnnualSalary { get; set; }
        public decimal? EstimatedHiringFee { get; set; }
        public List<string> EmploymentPreferences { get; set; }
        public TechnicalAndProfessionalTrainingDto TechnicalAndProfessionalTraining { get; set; }
        public HiringManagerTraitifyDto Traitify { get; set; }
        public List<LanguageDto> Languages { get; set; }
        public List<EducationDto> FormalEducation { get; set; }
        public List<WorkHistoryDto> WorkHistories { get; set; }
        public string VolunteerOrPassionProjects { get; set; }
    }

    public class HiringManagerTraitifyDto
    {
        public string PersonalityBlendName { get; set; }
        public string Personality1ImageUrl { get; set; }
        public string Personality2ImageUrl { get; set; }
    }

    public class TechnicalAndProfessionalTrainingDto
    {
        public string Institution { get; set; }
        public string Concentration { get; set; }
    }

    public class LanguageDto
    {
        public string Name { get; set; }
        public string Proficiency { get; set; }
    }

    public class EducationDto
    {
        public string DegreeType { get; set; }
        public string Institution { get; set; }
        public string Concentration { get; set; }
    }
    public class WorkHistoryDto
    {
        public string Position { get; set; }
        public string Company { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Description { get; set; }
    }
}
