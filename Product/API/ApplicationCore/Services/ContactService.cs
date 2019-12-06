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
            if(contactUsDto == null)
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
    }
}
