using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        public Guid PasswordResetRequestGuid { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        // todo: create regex to enforce pw requirements
        public string ConfirmPassword { get; set; }
    }
}
