using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class CreateJobPostingViewModel : BaseViewModel
    {
        public bool IsEdit { get; set; }
        public Guid EditGuid { get; set; }
        public string RequestPath { get; set; }
        public string ErrorMsg { get; set; }
 
 

        #region Basic job posting information 
        [Required(ErrorMessage = "Job title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Job description is required")]
        [StringLength(6000, MinimumLength = 300, ErrorMessage = "Job postings must contain between 300-6000 characters")]
        public string Description { get; set; }


        public string SelectedSkills { get; set; }
        public bool IsAgency { get; set; }

        public bool IsDraft { get; set; }


        [Range(0, 10000.00, ErrorMessage = "Percentage must be between 0 and 100")]
        public int? Telecommute { get; set; }

        [RegularExpression(@"^-?[0-9]*\.?[0-9]+$|^\s*$", ErrorMessage = "Compensation must be numeric")]
        public decimal? Compensation { get; set; }


        public DateTime ApplicationDeadline { get; set; }


        [Required(ErrorMessage = "Expiration date is required")]
        public DateTime PostingExpirationDate { get; set; }

        public Guid? SelectedIndustry { get; set; }
        public Guid? SelectedJobCategory { get; set; }
        public Guid? SelectedEducationLevel { get; set; }
        public Guid? SelectedExperienceLevel { get; set; }
        public Guid? SelectedEmploymentType { get; set; }
        public Guid? SelectedSecurityClearance { get; set; }

        public Guid? SelectedCompensationType { get; set; }

        [Required(ErrorMessage = "Company is required")]
        public Guid? SelectedRecruiterCompany { get; set; }

        #endregion

        #region Location Info
        [Required(ErrorMessage = "State/Provinc is required")]
        public string SelectedState { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
 
        [RegularExpression(@"^[0-9]{5}$|^[A-Z][0-9][A-Z] ?[0-9][A-Z][0-9]$", ErrorMessage = "Invalid Postal Code")] 
        public string PostalCode { get; set; }

        public string StreetAddress{ get; set; }


        public IList<SkillDto> Skills { get; set; }

        #endregion

        #region Select lists 



        public IEnumerable<SelectListItem> States { get; set; }

        public IEnumerable<SelectListItem> Industries { get; set; }


        public IEnumerable<SelectListItem> JobCategories { get; set; }

        public IEnumerable<SelectListItem> ExperienceLevels { get; set; }

        public IEnumerable<SelectListItem> EducationLevels { get; set; }

        public IEnumerable<SelectListItem> CompensationTypes { get; set; }

        public IEnumerable<SelectListItem> EmploymentTypes { get; set; }
        public IEnumerable<SelectListItem> SecurityClearances { get; set; }

        public IEnumerable<SelectListItem> RecruiterCompanies { get; set; }

        #endregion




    }
}
