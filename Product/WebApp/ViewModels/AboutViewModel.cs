using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        [RegularExpression("^([A-Za-z]+[,.]?[ ]?|[A-Za-z]+['-]?)+$")]
        public string ContactUsFirstName { get; set; }
        [RegularExpression("^([A-Za-z]+[,.]?[ ]?|[A-Za-z]+['-]?)+$")]
        public string ContactUsLastName { get; set; }
        [RegularExpression("[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$")]
        public string ContactUsEmail { get; set; }
        public string ContactUsType { get; set; }
        public string ContactUsComment { get; set; }
    }
}
