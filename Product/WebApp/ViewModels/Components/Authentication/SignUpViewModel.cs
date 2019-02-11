using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class SignUpViewModel : ComponentViewModel
    {
        public string SkewDirection { get; set; }
        [RegularExpression(@"^.+@[^\.].*\.[a-z]{2,}$", ErrorMessage = "Invalid email.")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string ReenterPassword { get; set; }
        public Guid ContactGuid { get; set; }
        public Guid? CampaignGuid { get; set; }
    }
}