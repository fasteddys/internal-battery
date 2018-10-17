using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public string ContactUsFirstName { get; set; }
        public string ContactUsLastName { get; set; }
        public string ContactUsEmail { get; set; }
        public string ContactUsType { get; set; }
        public string ContactUsComment { get; set; }
    }
}
