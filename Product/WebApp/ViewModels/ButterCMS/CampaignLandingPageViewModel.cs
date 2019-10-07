using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{
    public class CampaignLandingPageViewModel : BaseViewModel
    {
        public string hero_title { get; set; }
        public string hero_image { get; set; }
        public string hero_content { get; set; }
        public string hero_sub_image { get; set; }
        public string content_band_header { get; set; }
        public string content_band_text { get; set; }
        public bool iswaitlist { get; set; }
        public bool isLoggedIn { get; set; }
        public PartnerViewModel partner { get; set; }
        public string signup_form_header { get; set; }
        public string signup_form_text { get; set; }
        public string signup_form_submit_button_text { get; set; }
        public string success_header { get; set; }
        public string success_text { get; set; }
        public string existing_user_button_text { get; set; }
        public string existing_user_form_header { get; set; }
        public string existing_user_form_text { get; set; }
        public string existing_user_form_submit_button_text { get; set; }
        public string existing_user_success_header { get; set; }
        public string existing_user_success_text { get; set; }
        public bool isgateddownload { get; set; }
        public string gated_file_download_file { get; set; }
        public SignUpViewModel signUpViewModel { get; set; }
    }
}
