using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class SignupFlowViewModel : BaseViewModel
    {
        public IFormFile Resume { get; set; }
        public String WorkHistory { get; set; }
        public String EducationHistory { get; set; }
        public IList<SkillDto> Skills { get; set; }
        public string SelectedSkills { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "First name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public String FirstName { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "Last name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public String LastName { get; set; }
        public String Address { get; set; }
        public Guid? SelectedState { get; set; }
        public Guid? SelectedCountry { get; set; }
        public String City { get; set; }
        public String PostalCode { get; set; }
        public IEnumerable<SelectListItem> States { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
        public Guid SubscriberGuid { get; set; }
        private string _FormattedPhone;
        public string FormattedPhone
        {
            get
            {
                long phone = 0;
                if (long.TryParse(this._FormattedPhone, out phone))
                    return String.Format("{0:(###) ###-####}", phone);
                else
                    return _FormattedPhone;
            }
            set
            {
                if (value != null)
                    _FormattedPhone = value.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                else
                    _FormattedPhone = value;
            }
        }
        [RegularExpression("^[2-9]{1}[0-9]{9}$", ErrorMessage = "Phone must be 10 digits and may not start with a 0 or 1.")]
        public string Phone
        {
            get
            {
                if (this._FormattedPhone != null)
                    return this.FormattedPhone.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                else
                    return _FormattedPhone;
            }
        }
    }
}
