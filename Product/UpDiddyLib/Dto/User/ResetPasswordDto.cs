using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UpDiddyLib.Dto.User
{
    public class ResetPasswordDto
    {
        [Required]
        public Guid PasswordResetRequestGuid { get; set; }
        [Required]
        public string Password { get; set; }
        // todo: create regex to enforce pw requirements? should this validated in more than one place?
    }
}
