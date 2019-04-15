using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UpDiddy.ViewModels
{
    public class JobPostingViewModel : BaseViewModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        #region Location Info

        public Guid? SelectedState { get; set; }
        public Guid? SelectedIndustry { get; set; }
        public Guid? SelectedJobCategory { get; set; }
        public Guid? SelectedEducationLevel { get; set; }
        public Guid? SelectedExperienceLevel { get; set; }
        public Guid? SelectedEmploymentType { get; set; }
        public Guid? SelectedSecurityClearance { get; set; }

        public Guid? SelectedCompensationType{ get; set; }


        public IEnumerable<SelectListItem> States { get; set; }

        public IEnumerable<SelectListItem> Industries { get; set; }


        public IEnumerable<SelectListItem> JobCategories { get; set; }

        public IEnumerable<SelectListItem> ExperienceLevels { get; set; }

        public IEnumerable<SelectListItem> EducationLevels { get; set; }

        public IEnumerable<SelectListItem> CompensationTypes { get; set; }

        public IEnumerable<SelectListItem> EmploymentTypes { get; set; }
        public IEnumerable<SelectListItem> SecurityClearances { get; set; }

        public string City { get; set; }
 
        [RegularExpression(@"^[0-9]{5}$|^[A-Z][0-9][A-Z] ?[0-9][A-Z][0-9]$", ErrorMessage = "Invalid Postal Code")] 
        public string PostalCode { get; set; }

        public string StreetAddress{ get; set; }



        #endregion

        public string SelectedSkills { get; set; }
        public bool IsAgency { get; set; }

        public bool IsDraft { get; set; }

 
        [Range(0, 10000.00, ErrorMessage = "Percentage must be between 0 and 100")]
        public int? Telecommute { get; set; }
 
        [RegularExpression(@"^-?[0-9]*\.?[0-9]+$|^\s*$", ErrorMessage = "Compensation must be numeric")]
        public decimal? Compensation { get; set; }


    }
}
