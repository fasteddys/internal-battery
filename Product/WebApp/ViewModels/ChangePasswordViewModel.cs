using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Helpers.CustomDataAnnotations;

namespace UpDiddy.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        public Guid PasswordResetRequestGuid { get; set; }
        [Required]
        [ADB2CPasswordComplexityRequirement (ErrorMessage = "The password does not meet complexity requirements (1)")]
        [Auth0PasswordComplexityRequirement (ErrorMessage = "The password does not meet complexity requirements (2)")]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
