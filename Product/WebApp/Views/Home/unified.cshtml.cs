using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace B2C_Custom_Policy_Templates.Pages
{
    public class UnifiedModel : PageModel
    {
        public string TermsLabel { get; set; }

        public void OnGet()
        {
            string CultureName = Request.Query["ui_locales"].ToString();
            if (string.IsNullOrEmpty(CultureName))            
                CultureName = "en";

            TermsLabel = "Terms Of Service";

            if ( CultureName == "fr")
                    TermsLabel = "Termes et conditions";
                
            //  CultureName = "fr";

            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            // Culture contains the information of the requested culture
            var culture = rqf.RequestCulture.Culture;

            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(CultureName);
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(CultureName);
            //System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(CultureName);
            //System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(CultureName);
            //System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(CultureName);
            //System.Globalization.CultureInfo.CurrentUICulture= new System.Globalization.CultureInfo(CultureName);

            CultureInfo.CurrentCulture = new CultureInfo(CultureName, false); 
            CultureInfo.CurrentUICulture = new CultureInfo(CultureName, false);

        }




    }
}
