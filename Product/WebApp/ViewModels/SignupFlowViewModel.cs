using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class SignupFlowViewModel : BaseViewModel
    {
        public IFormFile Resume { get; set; }
        public String WorkHistory { get; set; }
        public String EducationHistory { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "First name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public String FirstName { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "Last name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public String LastName { get; set; }
        [RegularExpression(@"^\d$", ErrorMessage = "Please enter a valid phone number.")]    
        public String PhoneNumber { get; set; }
        [RegularExpression(@"\w+(\s\w+){2,}", ErrorMessage = "Please enter a valid street address.")]
        public String Address { get; set; }
    }
}
