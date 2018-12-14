using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class SignupFlowViewModel : BaseViewModel
    {
        public IFormFile Resume { get; set; }
    }
}
