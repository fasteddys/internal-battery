using System;
using System.Collections.Generic;
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

        public string PostalCode { get; set; }

        public string StreetAddress{ get; set; }


        public bool IsAgency { get; set; }
        #endregion


    }
}
