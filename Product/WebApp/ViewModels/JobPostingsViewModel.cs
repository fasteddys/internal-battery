using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class JobPostingsViewModel : BaseViewModel
    {
        public List<JobPostingDto> jobPostings { get; set; }     
    }
}
