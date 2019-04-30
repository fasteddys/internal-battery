using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels
{
    public class JobApplicationViewModel : BaseViewModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string CoverLetter { get; set; }
        public string Email { get; set; }
        public Guid JobPostingGuid { get; set; }
        public JobPostingDto Job { get; set; }
        public bool HasResumeOnFile { get; set; }
    }
}
