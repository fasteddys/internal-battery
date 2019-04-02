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
        public string HeroTitle { get; set; }
        public string HeroSubheader1 { get; set; }
        public string HeroSubheader2 { get; set; }
        public string HeroSubheader3 { get; set; }
    }
}
