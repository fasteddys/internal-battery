using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Configuration;
using System.Web;
using System.Text;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class ContactService : IContactService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IConfiguration _configuration;
        private readonly ISysEmail _sysEmail;
        public ContactService(IRepositoryWrapper repositoryWrapper, IConfiguration configuration, ISysEmail sysEmail)
        {
            _repositoryWrapper = repositoryWrapper;
            _sysEmail = sysEmail;
            _configuration = configuration;
        }

        public async Task CreateNewMessage(ContactUsDto contactUsDto)
        {
            if (contactUsDto == null)
                throw new NullReferenceException("ContactUsDto cannot be null");
            var subject = _configuration["SysEmail:ContactUs:Subject"];
            string firstName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(contactUsDto.FirstName) ? "No first name enetered." : contactUsDto.FirstName);
            string lastName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(contactUsDto.LastName) ? "No last name entered." : contactUsDto.LastName);
            string email = HttpUtility.HtmlEncode(string.IsNullOrEmpty(contactUsDto.Email) ? "No email entered." : contactUsDto.Email);
            string type = HttpUtility.HtmlEncode(string.IsNullOrEmpty(contactUsDto.ContactType) ? "No type entered." : contactUsDto.ContactType);
            string comment = HttpUtility.HtmlEncode(string.IsNullOrEmpty(contactUsDto.Message) ? "No comment entered." : contactUsDto.Message);
            var emailBody = FormatContactEmail(firstName, lastName, email, type, comment);
            await _sysEmail.SendEmailAsync(_configuration["SysEmail:ContactUs:Recipient"], subject, emailBody, Constants.SendGridAccount.Transactional);
        }

        public async Task CreateHireTalentMessage(HireTalentDto hireTalentDto)
        {
            var subject = _configuration["SysEmail:ContactUs:HireTalentSubject"];
            string companyName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.CompanyName) ? "No company name enetered." : hireTalentDto.CompanyName);
            string firstName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.FirstName) ? "No first name enetered." : hireTalentDto.FirstName);
            string lastName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.LastName) ? "No last name entered." : hireTalentDto.LastName);
            string title = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.Title) ? "No title name entered." : hireTalentDto.Title);
            string email = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.Email) ? "No email entered." : hireTalentDto.Email);
            string phone = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.Phone) ? "No phone entered." : hireTalentDto.Phone);
            string pipeline = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.Pipeline) ? "No pipeline entered." : hireTalentDto.Pipeline);
            string skillset = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.SkillSet) ? "No skill set entered." : hireTalentDto.SkillSet);
            string location = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.Location) ? "No location entered." : hireTalentDto.Location);
            string comments = HttpUtility.HtmlEncode(string.IsNullOrEmpty(hireTalentDto.Comments) ? "No comment entered." : hireTalentDto.Comments);
            var emailBody = FormatHireTalentEmail(companyName, firstName, lastName, title, email, phone, pipeline, skillset, location, comments);
            await _sysEmail.SendEmailAsync(_configuration["SysEmail:ContactUs:HireTalentRecipient"], subject, emailBody, Constants.SendGridAccount.Transactional);
        }

        private string FormatContactEmail(string ContactUsFirstName,
           string ContactUsLastName,
           string ContactUsEmail,
           string ContactUsType,
           string ContactUsComment)
        {
            StringBuilder emailBody = new StringBuilder("<strong>New email submitted by: </strong>" + ContactUsLastName + ", " + ContactUsFirstName);
            emailBody.Append("<p><strong>User's email: </strong>" + ContactUsEmail + "</p>");
            emailBody.Append("<p><strong>Contact topic: </strong>" + ContactUsType + "</p>");
            emailBody.Append("<p><strong>Message: </strong></p>");
            emailBody.Append("<p>" + ContactUsComment + "</p>");
            return emailBody.ToString();
        }

        private string FormatHireTalentEmail(string companyName,
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

    }
}
