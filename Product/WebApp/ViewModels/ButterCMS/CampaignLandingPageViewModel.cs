using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{
    public class CampaignLandingPageViewModel : SignUpViewModel
    {
        public string hero_title { get; set; }
        public string hero_image { get; set; }
        public string hero_subheader_1 { get; set; }
        public string hero_subheader_2 { get; set; }
        public string hero_subheader_3 { get; set; }
        public string content_band_header { get; set; }
        public string content_band_text { get; set; }
    }
}
