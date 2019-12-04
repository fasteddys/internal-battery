using System.Collections.Generic;
using System.Threading.Tasks;
using ButterCMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.Controllers;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using UpDiddyLib.Helpers;
using UpDiddy.ViewModels;
using System.Web;
using System.Text;
namespace WebApp.Controllers
{
    public class HireTalentController : BaseController
    {
        private IButterCMSService _butterService;
        private readonly ISysEmail _sysEmail;

        public HireTalentController(IApi api,
        IConfiguration configuration,
        IButterCMSService butterService,
        ISysEmail sysEmail)
         : base(api, configuration)
        {
            _butterService = butterService;
            _sysEmail = sysEmail;
        }

        [HttpGet("hire-talent")]
        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<HireTalentPageViewModel> hireTalentPage = await _butterService.RetrievePageAsync<HireTalentPageViewModel>("/hire-talent", QueryParams);

            if (hireTalentPage == null)
                return NotFound();

            SetSEOTags(hireTalentPage);

            HireTalentViewModel model = new HireTalentViewModel()
            {
                Header = hireTalentPage.Data.Fields.Header,
                Content = hireTalentPage.Data.Fields.Content,
                ContactFormHeader = hireTalentPage.Data.Fields.ContactFormHeader,
                ContactFormText = hireTalentPage.Data.Fields.ContactFormText
            };

            return View(model);
        }

        [HttpPost("hire-talent/Contact")]
        public async Task<IActionResult> Contact(HireTalentViewModel model)
        {

            var subject = _configuration["SysEmail:ContactUs:HireTalentSubject"];
            string companyName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.CompanyName) ? "No company name enetered." : model.CompanyName);
            string firstName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.FirstName) ? "No first name enetered." : model.FirstName);
            string lastName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.LastName) ? "No last name entered." : model.LastName);
            string title = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.Title) ? "No title name entered." : model.Title);
            string email = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.Email) ? "No email entered." : model.Email);
            string phone = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.Phone) ? "No phone entered." : model.Phone);
            string pipeline = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.Pipeline) ? "No pipeline entered." : model.Pipeline);
            string skillset = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.SkillSet) ? "No skill set entered." : model.SkillSet);
            string location = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.Location) ? "No location entered." : model.Location);
            string comments = HttpUtility.HtmlEncode(string.IsNullOrEmpty(model.Comments) ? "No comment entered." : model.Comments);
            var emailBody = FormatContactEmail(companyName, firstName, lastName, title, email, phone, pipeline, skillset, location, comments);
            await _sysEmail.SendEmailAsync(_configuration["SysEmail:ContactUs:Recipient"], subject, emailBody, Constants.SendGridAccount.Transactional);
            return View("MessageReceived");
        }

        private string FormatContactEmail(string companyName,
        string firstName,
        string lastName,
        string title,
        string email,
        string phone,
        string pipeline,
        string skillset,
        string location,
        string comments)
        {
            StringBuilder emailBody = new StringBuilder("<strong>New email submitted by: </strong>" + firstName + ", " + lastName);
            emailBody.Append("<p><strong>Company name: </strong>" + companyName + "</p>");
            emailBody.Append("<p><strong>Title: </strong>" + title + "</p>");
            emailBody.Append("<p><strong>Email: </strong>" + email + "</p>");
            emailBody.Append("<p><strong>Phone: </strong>" + phone + "</p>");
            emailBody.Append("<p><strong>Pipeline: </strong>" + pipeline + "</p>");
            emailBody.Append("<p><strong>Skill Set: </strong>" + skillset + "</p>");
            emailBody.Append("<p><strong>Location: </strong>" + location + "</p>");
            emailBody.Append("<p><strong>Comments: </strong></p>");
            emailBody.Append("<p>" + comments + "</p>");
            return emailBody.ToString();
        }

        private void SetSEOTags(PageResponse<HireTalentPageViewModel> landingPage)
        {
            ViewData[Constants.Seo.TITLE] = landingPage.Data.Fields.Title;
            ViewData[Constants.Seo.META_DESCRIPTION] = landingPage.Data.Fields.MetaDescription;
            ViewData[Constants.Seo.META_KEYWORDS] = landingPage.Data.Fields.MetaKeywords;
            ViewData[Constants.Seo.OG_TITLE] = landingPage.Data.Fields.OpenGraphTitle;
            ViewData[Constants.Seo.OG_DESCRIPTION] = landingPage.Data.Fields.OpenGraphDescription;
            ViewData[Constants.Seo.OG_IMAGE] = landingPage.Data.Fields.OpenGraphImage;
        }
    }
}