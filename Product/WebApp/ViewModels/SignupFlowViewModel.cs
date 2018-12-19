using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class SignupFlowViewModel : BaseViewModel
    {
        public IFormFile Resume { get; set; }
        public String WorkHistory { get; set; }
        public String EducationHistory { get; set; }
        public IList<SkillDto> Skills { get; set; }
        public string SelectedSkills { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "First name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public String FirstName { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "Last name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public String LastName { get; set; }
        [RegularExpression(@"^\d$", ErrorMessage = "Please enter a valid phone number.")]    
        public String PhoneNumberFirstThree { get; set; }
        [RegularExpression(@"^\d$", ErrorMessage = "Please enter a valid phone number.")]
        public String PhoneNumberSecondThree { get; set; }
        [RegularExpression(@"^\d$", ErrorMessage = "Please enter a valid phone number.")]
        public String PhoneNumberLastFour { get; set; }
        [RegularExpression(@"\w+(\s\w+){2,}", ErrorMessage = "Please enter a valid street address.")]
        public String Address { get; set; }
        public Guid? SelectedState { get; set; }
        public Guid? SelectedCountry { get; set; }
        public IEnumerable<SelectListItem> States { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
    }
}
