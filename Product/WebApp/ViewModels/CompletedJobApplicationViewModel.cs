using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class CompletedJobApplicationViewModel : BaseViewModel
    {
        public enum ApplicationStatus { Success, Failed};
        public ApplicationStatus JobApplicationStatus { get; set; }
        public string Description { get; set; }

    }
}
