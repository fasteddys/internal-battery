using System;
using System.ComponentModel.DataAnnotations;
using UpDiddyLib.Helpers.CustomDataAnnotations;

namespace UpDiddyLib.Dto.User
{
    public class ResetPasswordDto
    {
        [Required]
        public Guid PasswordResetRequestGuid { get; set; }
        [Required]
        [ADB2CPasswordComplexityRequirement(ErrorMessage = "The password does not meet complexity requirements (1)")]
        [Auth0PasswordComplexityRequirement(ErrorMessage = "The password does not meet complexity requirements (2)")]
        public string Password { get; set; }        
    }
}
