using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class ResumeViewModel
    {
        [Required]
        public IFormFile Resume { get; set; }
    }
}
