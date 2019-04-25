using System;
using System.Collections.Generic;
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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CoverLetter { get; set; }
        public Guid JobPostingGuid { get; set; }
        public JobPostingDto Job { get; set; }
        public bool HasResumeOnFile { get; set; }
    }
}
